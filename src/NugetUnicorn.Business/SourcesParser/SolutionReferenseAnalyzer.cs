using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
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
            _solutionPath = solutionPath;
        }

        public IObservable<Message.Info> Run()
        {
            var solutionPath = NormalizeSolutionPath();
            return Observable.Create<Message.Info>(
                x =>
                {
                    return _scheduler.ScheduleAsync(
                        async (scheduler, cancellationToken) =>
                                await AnalyzeSolution(solutionPath, x, cancellationToken));
                });
        }

        private string NormalizeSolutionPath()
        {
            var solutionPath = _solutionPath;
            if (!Path.IsPathRooted(_solutionPath))
            {
                var location = Assembly.GetEntryAssembly().Location;
                var directoryName = Path.GetDirectoryName(location);
                solutionPath = Path.Combine(directoryName, solutionPath);
            }
            return solutionPath;
        }

        private static Task AnalyzeSolution(string solutionPath, IObserver<Message.Info> observer,
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

                var projectReferenceVsDirectDllReference = ComposeProjectReferenceErrors(referenceMetadatas,
                    wrongReferenceMatcher);
                var incorrectReferences = ComposeBindingsErrors(projects, referenceMetadatas, observer);

                var errorReport = projectReferenceVsDirectDllReference.Merge(incorrectReferences);
                foreach (var item in errorReport)
                {
                    observer.OnNextInfo($"project: {item.Key} report:");
                    try
                    {
                        item.Value
                            .DoIfEmpty(() => observer.OnNextInfo("all seems to be ok"))
                            .Do(x => observer.OnNextError($"possible issue: {x}"));
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

        private static IDictionary<ProjectPoco, IEnumerable<string>> ComposeProjectReferenceErrors(
            IDictionary<ProjectPoco, IEnumerable<ReferenceMetadataBase>> referenceMetadatas,
            IProbabilityMatchEngine<DllMetadata> wrongReferenceMatcher)
        {
            return referenceMetadatas.Transform(x => x.OfType<DllMetadata>())
                .Transform(
                    x =>
                        x
                            .FindBestMatch
                            <DllMetadata, WrongReferencePropabilityMetadata
                            >(
                                wrongReferenceMatcher,
                                0d))
                .Transform(
                    x => x.Select(
                        y =>
                                $"found possible misreference: {y.Reference} (solution contains project with the same target name: {y.SuspectedProject.Name} / {y.SuspectedProject.TargetName})"));
        }

        private static IEnumerable<KeyValuePair<ProjectPoco, IEnumerable<string>>> ComposeBindingsErrors(
            IList<ProjectPoco> projects, IDictionary<ProjectPoco, IEnumerable<ReferenceMetadataBase>> referenceMetadatas,
            IObserver<Message.Info> observer)
        {
            var referencesByProjects = referenceMetadatas.Transform(x => x.OfType<ExistingReferenceMetadataBase>())
                .Transform((x, y) => y.Select(z => ComposeReferenceInformation(z, x, observer)).Where(z => z != null));

            var appConfigFileParser = new ProbabilityMatchEngine<ProjectItem>();
            appConfigFileParser.With(new AppConfigFileReferenceMatcher());
            var projectBindings = projects.Where(x => x.AppConfigPath != null)
                .ToDictionary(
                    x => x,
                    x =>
                    {
                        var r = new AppConfigFileReferenceMatcher.AppConfigFilePropabilityMetadata(null, null, 0d,
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

        private static ReferenceInformation ComposeReferenceInformation(ExistingReferenceMetadataBase z, ProjectPoco x,
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