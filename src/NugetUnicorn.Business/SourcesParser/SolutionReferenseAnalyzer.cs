using System;
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
using NugetUnicorn.Business.Microsoft.Build;

namespace NugetUnicorn.Business.SourcesParser
{
    public class SolutionReferenseAnalyzer
    {
        private readonly IScheduler _scheduler;

        private readonly string _solutionPath;

        public SolutionReferenseAnalyzer(NewThreadScheduler scheduler, string solutionPath)
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

        private Task AnalyzeSolution(string solutionPath, IObserver<string> observer, CancellationToken disposable)
        {
            return Task.Run(() => AnalyzeInternal(solutionPath, observer), disposable);
        }

        private void AnalyzeInternal(string solutionPath, IObserver<string> observer)
        {
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

            var nugetPackageFileParser = new ProbabilityMatchEngine<ProjectItemInstance>();
            nugetPackageFileParser.With(new NugetPackageFileMatcher());

            projects.Select(
                x =>
                    {
                        observer.OnNext($"starting analysis of the {x.GetProjectName()} project...");
                        return x.Items;
                    })
                    .SelectMany(x => x)
                    .FindBestMatch<ProjectItemInstance, ReferenceMatcher.DllReference.DllMetadata>(referenceMatcher, 0d)
                    .FindBestMatch<ReferenceMatcher.DllReference.DllMetadata, WrongReferenceMatcher.WrongReferencePropabilityMetadata>(wrongReferenceMatcher, 0d)
                    .Do(x => observer.OnNext($"found possible misreference: {x.Project.GetProjectName()} to {x.Reference} (solution contains project with the same target name: {x.SuspectedProject.GetProjectName()} / {x.SuspectedProject.GetTargetFileName()})"));
            observer.OnNext("analysis completed.");
            observer.OnCompleted();
        }
    }
}