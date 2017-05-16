using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

using NugetUnicorn.Business.Extensions;
using NugetUnicorn.Business.FuzzyMatcher.Engine;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.Analyzer;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.Metadata;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.ReferenceType;
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

                var projectReferenceVsDirectDllReference = DllAndProjectMisreferenceAmalyzer.ComposeProjectReferenceErrors(referenceMetadatas, wrongReferenceMatcher);
                var referencesByProjects = ComposeReferencesByProjects(observer, referenceMetadatas);

                var incorrectReferences = IncorrectBindingsAnalyzer.ComposeBindingsErrors(projects, referencesByProjects);
                var differentVersionsReferencesErrors = DifferentVersionsAnalyzer.ComposeDifferentVersionsReferencesErrors(referencesByProjects);
                //[DS]: this world is just not readey yet for this check...
                //var assemblyRedirectsVsReferencesErrors = IncorrectRedirectsAndReferencesAnalyzer.ComposeRedirectsVsReferencesErrors(projects);
                var solutionDirectory = Path.GetDirectoryName(solutionPath);
                var inconsistentNugetPackageReferences = InconsistentNugetPackagesAnalyzer.ComposeInconsistentNugetPackageReferences(
                    solutionDirectory,
                    projects,
                    referencesByProjects,
                    referenceMetadatas);

                var observations = projectReferenceVsDirectDllReference.Merge(incorrectReferences)
                                                                       .Merge(differentVersionsReferencesErrors)
                                                                       //.Merge(assemblyRedirectsVsReferencesErrors)
                                                                       .Merge(inconsistentNugetPackageReferences)
                                                                       .Select(
                                                                           x =>
                                                                               {
                                                                                   if (x.Value.Any())
                                                                                   {
                                                                                       var errorMessage = string.Join(
                                                                                           Environment.NewLine,
                                                                                           x.Value.Select(y => $"possible issue: {y}"));
                                                                                       return new Message.Error(
                                                                                           $"project: {x.Key} report:{Environment.NewLine}{errorMessage}");
                                                                                   }
                                                                                   return new Message.Info($"project: {x.Key} report: all seems to be ok");
                                                                               });
                observations.ForEachItem(observer.OnNext);
            }
            catch (Exception e)
            {
                observer.OnNextError(e.Message);
            }
            finally
            {
                observer.OnNext(new Message.Info("analysis completed."));
                observer.OnCompleted();
            }
        }

        private static IDictionary<ProjectPoco, IEnumerable<ReferenceInformation>> ComposeReferencesByProjects(
            IObserver<Message.Info> observer,
            Dictionary<ProjectPoco, IEnumerable<ReferenceMetadataBase>>
                referenceMetadatas)
        {
            return referenceMetadatas.Transform(x => x.OfType<ExistingReferenceMetadataBase>())
                                     .Transform((x, y) => y.Select(z => ComposeReferenceInformation(z, x, observer)).Where(z => z != null));
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