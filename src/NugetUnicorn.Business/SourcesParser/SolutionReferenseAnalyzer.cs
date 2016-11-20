using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Build.Evaluation;

using NugetUnicorn.Business.Extensions;
using NugetUnicorn.Business.FuzzyMatcher.Engine;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.Analyzer;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.Metadata;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.ReferenceType;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.SolutionFileParsers;
using NugetUnicorn.Business.SourcesParser.ProjectParser;
using NugetUnicorn.Business.SourcesParser.ProjectParser.Structure;

using ProjectReference = NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.ReferenceType.ProjectReference;

namespace NugetUnicorn.Business.SourcesParser
{
    public class SolutionReferenseAnalyzer
    {
        private readonly IScheduler _scheduler;

        private readonly string _solutionPath;

        public SolutionReferenseAnalyzer(IScheduler scheduler, string solutionPath)
        {
            _scheduler = scheduler;
            _solutionPath = Path.GetFullPath(solutionPath);
        }

        public IObservable<Message.Info> Run()
        {
            return Observable.Create<Message.Info>(
                x =>
                    {
                        return _scheduler.ScheduleAsync(
                            async (scheduler, cancellationToken) =>
                            await AnalyzeSolution(_solutionPath, x, cancellationToken));
                    });
        }

        private static Task AnalyzeSolution(string solutionPath,
                                            IObserver<Message.Info> observer,
                                            CancellationToken disposable)
        {
            return Task.Run(() => AnalyzeInternal(solutionPath, observer), disposable);
        }

        private static void AnalyzeInternal(string solutionPath, IObserver<Message.Info> observer)
        {
            try
            {
                observer.OnNextInfo("--==--");

                observer.OnNextInfo($"starting the {solutionPath} analysis...");
                observer.OnNextInfo("parsing the solution...");
                var projects = SolutionParser.GetProjects(solutionPath)
                                             .ToList();

                observer.OnNextInfo("parsed.");

                var referenceMatcher = new ProbabilityMatchEngine<ReferenceBase>();
                referenceMatcher.With(new NugetReference())
                                .With(new SystemReference())
                                .With(new DllReference())
                                .With(new ProjectReference());

                var wrongReferenceMatcher = new ProbabilityMatchEngine<DllMetadata>();
                wrongReferenceMatcher.With(new WrongReferenceMatcher(projects));

                var referenceMetadatas = projects.ToDictionary(
                    x => x,
                    x => x.References.FindBestMatch<ReferenceBase, ReferenceMetadataBase>(referenceMatcher, 0d));

                var projectReferenceVsDirectDllReference = ComposeProjectReferenceErrors(referenceMetadatas, wrongReferenceMatcher);
                var referencesByProjects = ComposeReferencesByProjects(observer, referenceMetadatas);

                var incorrectReferences = ComposeBindingsErrors(projects, referencesByProjects);
                var differentVersionsReferencesErrors = ComposeDifferentVersionsReferencesErrors(referencesByProjects);

                var errorReport = projectReferenceVsDirectDllReference.Merge(incorrectReferences)
                                                                      .Merge(differentVersionsReferencesErrors);
                foreach (var item in errorReport)
                {
                    try
                    {
                        if (item.Value.Any())
                        {
                            var errorMessage = string.Join(Environment.NewLine, item.Value.Select(x => $"possible issue: {x}"));
                            observer.OnNextError($"project: {item.Key} report:{Environment.NewLine}{errorMessage}");
                        }
                        else
                        {
                            observer.OnNextInfo($"project: {item.Key} report: all seems to be ok");
                        }
                    }
                    catch (Exception e)
                    {
                        observer.OnNextError(e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                observer.OnNextError(e.Message);
            }
            finally
            {
                observer.OnNextInfo("analysis completed.");
                observer.OnCompleted();
            }
        }

        private static IDictionary<ProjectPoco, IEnumerable<ReferenceInformation>> ComposeReferencesByProjects(IObserver<Message.Info> observer,
                                                                                                               Dictionary<ProjectPoco, IEnumerable<ReferenceMetadataBase>>
                                                                                                                   referenceMetadatas)
        {
            return referenceMetadatas.Transform(x => x.OfType<ExistingReferenceMetadataBase>())
                                     .Transform((x, y) => y.Select(z => ComposeReferenceInformation(z, x, observer)).Where(z => z != null));
        }

        private static IDictionary<ProjectPoco, IEnumerable<string>> ComposeDifferentVersionsReferencesErrors(
            IDictionary<ProjectPoco, IEnumerable<ReferenceInformation>> referenceMetadatas)
        {
            return referenceMetadatas.SelectMany(
                x => x.Value.Select(y => new Tuple<ProjectPoco, string, string>(x.Key, y.AssemblyName, y.Version)))
                                     .GroupBy(x => x.Item2)
                                     .Select(x => new Tuple<string, IList<IGrouping<string, Tuple<ProjectPoco, string, string>>>>(x.Key, x.GroupBy(y => y.Item3).ToList()))
                                     .Where(x => x.Item2.Count > 1)
                                     .Select(
                                         x =>
                                         new Tuple<string, IEnumerable<Tuple<string, IEnumerable<ProjectPoco>>>>(
                                             x.Item1,
                                             x.Item2.Select(y => new Tuple<string, IEnumerable<ProjectPoco>>(y.Key, y.Select(z => z.Item1)))))
                                     .Select(
                                         x =>
                                         new Tuple<string, IEnumerable<Tuple<string, ProjectPoco>>>(
                                             x.Item1,
                                             x.Item2.SelectMany(y => y.Item2.Select(z => new Tuple<string, ProjectPoco>(y.Item1, z)))))
                                     .Select(x => new Tuple<string, IEnumerable<KeyValuePair<ProjectPoco, string>>>(x.Item1, ComposeProjectMessage(x.Item2, x.Item1)))
                                     .SelectMany(x => x.Item2)
                                     .GroupBy(x => x.Key)
                                     .ToDictionary(x => x.Key, x => x.Select(y => y.Value));
        }

        private static IEnumerable<KeyValuePair<ProjectPoco, string>> ComposeProjectMessage(IEnumerable<Tuple<string, ProjectPoco>> versionsAndProjects, string referenceName)
        {
            var enumerable = versionsAndProjects.ToArray();
            return
                enumerable.Select(
                    x =>
                    new KeyValuePair<ProjectPoco, string>(
                        x.Item2,
                        $"{x.Item2} reference {referenceName} {x.Item1} but there are projects which has same reference but with different version: {string.Join(", ", enumerable.Where(y => y.Item1 != x.Item1 && y.Item2 != x.Item2).Select(y => $"{y.Item2} -> {referenceName} v {y.Item1}"))}"
                        ));
        }

        private static IDictionary<ProjectPoco, IEnumerable<string>> ComposeProjectReferenceErrors(
            IDictionary<ProjectPoco, IEnumerable<ReferenceMetadataBase>> referenceMetadatas,
            IProbabilityMatchEngine<DllMetadata> wrongReferenceMatcher)
        {
            return referenceMetadatas.Transform(x => x.OfType<DllMetadata>())
                                     .Transform(
                                         x =>
                                         x.FindBestMatch<DllMetadata, WrongReferencePropabilityMetadata>(wrongReferenceMatcher, 0d))
                                     .Transform(
                                         x => x.Select(
                                             y =>
                                             $"found possible misreference: {y.Reference} (solution contains project with the same target name: {y.SuspectedProject.Name} / {y.SuspectedProject.TargetName})"));
        }

        private static IEnumerable<KeyValuePair<ProjectPoco, IEnumerable<string>>> ComposeBindingsErrors(
            IList<ProjectPoco> projects,
            IDictionary<ProjectPoco, IEnumerable<ReferenceInformation>> referencesByProjects)
        {
            var appConfigFileParser = new ProbabilityMatchEngine<ProjectItem>();
            appConfigFileParser.With(new AppConfigFileReferenceMatcher());
            var projectBindings = projects.Where(x => x.AppConfigPath != null)
                                          .ToDictionary(
                                              x => x,
                                              x =>
                                                  {
                                                      var r = new AppConfigFileReferenceMatcher.AppConfigFilePropabilityMetadata(
                                                          null,
                                                          null,
                                                          0d,
                                                          x.AppConfigPath);
                                                      return r.RedirectModels;
                                                  });

            return projectBindings.Join(
                referencesByProjects,
                x => x.Key,
                y => y.Key,
                (x, y) =>
                    {
                        var bindingReferences = x.Value.Select(z => new ReferenceInformation(z.Name, z.NewVersion));

                        var incorrect = bindingReferences.Join(
                            y.Value,
                            x1 => x1.AssemblyName,
                            y1 => y1.AssemblyName,
                            (x1, y1) => new Tuple<ReferenceInformation, ReferenceInformation>(x1, y1))
                                                         .Where(z => !string.Equals(z.Item1.Version, z.Item2.Version))
                                                         .Select(
                                                             z => $"reference mismatch: redirect: {z.Item1.ToString()}, reference: {z.Item2.ToString()}");

                        return new KeyValuePair<ProjectPoco, IEnumerable<string>>(x.Key, incorrect);
                    });
        }

        private static ReferenceInformation ComposeReferenceInformation(ExistingReferenceMetadataBase z,
                                                                        ProjectPoco x,
                                                                        IObserver<Message.Info> observer)
        {
            try
            {
                return z.GetReferenceInformation(x);
            }
            catch (Exception e)
            {
                observer.OnNextError(e.Message);
                return null;
            }
        }
    }
}