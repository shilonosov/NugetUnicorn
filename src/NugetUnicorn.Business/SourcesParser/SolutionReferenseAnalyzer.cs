using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Build.Execution;

using NugetUnicorn.Business.Extensions;
using NugetUnicorn.Business.FuzzyMatcher.Engine;
using NugetUnicorn.Business.FuzzyMatcher.Matchers;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.SolutionFileParsers;
using NugetUnicorn.Business.Microsoft.Build;

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

        public IObservable<string> Subscribe()
        {
            return Observable.Create<string>(
                x =>
                    {
                        return _scheduler.ScheduleAsync(
                            async (scheduler, cancellationToken) => await AnalyzeSolution(_solutionPath, x, cancellationToken));
                    });
        }

        private static Task AnalyzeSolution(string solutionPath, IObserver<string> observer, CancellationToken disposable)
        {
            return Task.Run(() => AnalyzeInternal(solutionPath, observer), disposable);
        }

        private static void AnalyzeInternal(string solutionPath, IObserver<string> observer)
        {
            observer.OnNext("--==--");

            observer.OnNext($"starting the {solutionPath} analysis...");
            observer.OnNext("parsing the solution...");
            var projects = SolutionParser.GetProjects(solutionPath)
                                         .ToList();

            observer.OnNext("parsed.");

            var referenceMatcher = new ProbabilityMatchEngine<ProjectItemInstance>();
            referenceMatcher.With(new ReferenceMatcher.NugetReference())
                            .With(new ReferenceMatcher.SystemReference())
                            .With(new ReferenceMatcher.ExplicitReference())
                            .With(new ReferenceMatcher.DllReference())
                            .With(new ReferenceMatcher.ProjectReference());

            var wrongReferenceMatcher = new ProbabilityMatchEngine<ReferenceMatcher.DllReference.DllMetadata>();
            wrongReferenceMatcher.With(new WrongReferenceMatcher(projects));

            var referenceMetadatas = projects.ToDictionary(
                x => x.GetProjectName(),
                x => x.Items.FindBestMatch<ProjectItemInstance, ReferenceMatcher.ReferenceMetadataBase>(referenceMatcher, 0d));

            var projectReferenceVsDirectDllReference = ComposeProjectReferenceErrors(referenceMetadatas, wrongReferenceMatcher);
            var incorrectReferences = ComposeBindingsErrors(projects, referenceMetadatas);

            var errorReport = projectReferenceVsDirectDllReference.Merge(incorrectReferences);
            foreach (var item in errorReport)
            {
                observer.OnNext($"project: {item.Key} report:");
                item.Value
                    .DoIfEmpty(() => observer.OnNext("all seems to be ok"))
                    .Do(x => observer.OnNext($"possible issue: {x}"));
            }

            observer.OnNext("analysis completed.");
            observer.OnNext("");
            observer.OnCompleted();
        }

        private static IDictionary<string, IEnumerable<string>> ComposeProjectReferenceErrors(
            IDictionary<string, IEnumerable<ReferenceMatcher.ReferenceMetadataBase>> referenceMetadatas,
            IProbabilityMatchEngine<ReferenceMatcher.DllReference.DllMetadata> wrongReferenceMatcher)
        {
            return referenceMetadatas.Transform(x => x.OfType<ReferenceMatcher.DllReference.DllMetadata>())
                                     .Transform(
                                         x =>
                                         x
                                             .FindBestMatch
                                             <ReferenceMatcher.DllReference.DllMetadata, WrongReferenceMatcher.WrongReferencePropabilityMetadata
                                             >(
                                                 wrongReferenceMatcher,
                                                 0d))
                                     .Transform(
                                         x => x.Select(
                                             y =>
                                             $"found possible misreference: {y.Reference} (solution contains project with the same target name: {y.SuspectedProject.GetProjectName()} / {y.SuspectedProject.GetTargetFileName()})"));
        }

        private static IEnumerable<KeyValuePair<string, IEnumerable<string>>> ComposeBindingsErrors(IList<ProjectInstance> projects,
                                                                                                    IDictionary<string, IEnumerable<ReferenceMatcher.ReferenceMetadataBase>>
                                                                                                        referenceMetadatas)
        {
            var referencesByProjects = referenceMetadatas.Transform(x => x.OfType<ReferenceMatcher.ExistingReferenceMetadataBase>())
                                                         .Transform(x => x.Select(y => y.GetReferenceInformation()));

            var appConfigFileParser = new ProbabilityMatchEngine<ProjectItemInstance>();
            appConfigFileParser.With(new AppConfigFileReferenceMatcher());
            var projectBindings = projects.ToDictionary(
                x => x.GetProjectName(),
                x => x.Items.FindBestMatch<ProjectItemInstance, AppConfigFileReferenceMatcher.AppConfigFilePropabilityMetadata>(appConfigFileParser, 0d))
                                          .Transform(x => x.SelectMany(y => y.RedirectModels));

            return projectBindings.Join(
                referencesByProjects,
                x => x.Key,
                y => y.Key,
                (x, y) =>
                    {
                        var bindingReferences = x.Value.Select(z => new ReferenceMatcher.ReferenceInformation(z.Name, z.NewVersion));

                        var incorrect = bindingReferences.Join(
                            y.Value,
                            x1 => x1.AssemblyName,
                            y1 => y1.AssemblyName,
                            (x1, y1) => new Tuple<ReferenceMatcher.ReferenceInformation, ReferenceMatcher.ReferenceInformation>(x1, y1))
                                                         .Where(z => !string.Equals(z.Item1.Version, z.Item2.Version))
                                                         .Select(z => $"reference mismatch: redirect: {z.Item1.ToString()}, reference: {z.Item2.ToString()}");

                        return new KeyValuePair<string, IEnumerable<string>>(x.Key, incorrect);
                    });
        }
    }
}