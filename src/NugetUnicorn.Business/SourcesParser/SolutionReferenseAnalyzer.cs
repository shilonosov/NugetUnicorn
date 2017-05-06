using System;
using System.Collections.Generic;
using System.Diagnostics;
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

using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.ProjectManagement;

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
                //[DS]: this world is just not readey yet for this check...
                //var assemblyRedirectsVsReferencesErrors = ComposeRedirectsVsReferencesErrors(projects);
                var solutionDirectory = Path.GetDirectoryName(solutionPath);
                var inconsistentNugetPackageReferences = ComposeInconsistentNugetPackageReferences(solutionDirectory, projects, referencesByProjects, referenceMetadatas);

                projectReferenceVsDirectDllReference.Merge(incorrectReferences)
                                                    .Merge(differentVersionsReferencesErrors)
                                                    //.Merge(assemblyRedirectsVsReferencesErrors)
                                                    .Merge(inconsistentNugetPackageReferences)
                                                    .ToObservable()
                                                    .Select(
                                                        x =>
                                                            {
                                                                if (x.Value.Any())
                                                                {
                                                                    var errorMessage = string.Join(Environment.NewLine, x.Value.Select(y => $"possible issue: {y}"));
                                                                    return new Message.Error($"project: {x.Key} report:{Environment.NewLine}{errorMessage}");
                                                                }
                                                                return new Message.Info($"project: {x.Key} report: all seems to be ok");
                                                            })
                                                    .Catch<Message.Info, Exception>(x => Observable.Return<Message.Info>(new Message.Error(x)))
                                                    .Subscribe(observer);
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

        private static IEnumerable<KeyValuePair<ProjectPoco, IEnumerable<string>>> ComposeInconsistentNugetPackageReferences(
            string solutionPath,
            IList<ProjectPoco> projects,
            IDictionary<ProjectPoco, IEnumerable<ReferenceInformation>> referencesByProjects,
            IDictionary<ProjectPoco, IEnumerable<ReferenceMetadataBase>> referenceMetadatas)
        {
            var packagesConfigParser = PackagesConfigParser.Instance;
            return projects.Select(
                x =>
                    {
                        using (var pcs = File.Open(x.PackagesConfigPath, FileMode.Open))
                        {
                            var pcr = new PackagesConfigReader(pcs);
                            return pcr.GetPackages()
                                .Select(
                                y =>
                                    {
                                        var packageIdentity = y.PackageIdentity;
                                        var packagePathResolver = new PackagePathResolver(Path.Combine(solutionPath, "packages"));
                                        var packageDirectory = packagePathResolver
                                            .GetInstallPath(packageIdentity);
                                        var packageFileName = packagePathResolver.GetPackageFileName(packageIdentity);
                                        //var dlls = Directory.GetFiles(packagesPath, "*.dll");

                                        var packagePath = Path.Combine(packageDirectory, packageFileName);
                                        using (var nupkgStream = File.Open(packagePath, FileMode.Open))
                                        {
                                            var nupkgReader = new PackageArchiveReader(nupkgStream);
                                            var referenceItems = nupkgReader.GetReferenceItems()
                                                .ToArray();

                                            //var nugetFramefork = NuGetFramework.ParseFrameworkName(x.TargetFramework.Version, DefaultFrameworkNameProvider.Instance);
                                            var nugetFramefork = NuGetFramework.Parse(x.TargetFramework.Version);
                                            var r = GetMostCompatibleGroup(nugetFramefork, referenceItems);

                                            var nsr = nupkgReader.NuspecReader;
                                            Debug.WriteLine(nugetFramefork.GetShortFolderName());
                                            var frameworkReferenceGroups = nsr.GetFrameworkReferenceGroups()
                                                                              .ToArray();
                                            Debug.WriteLine(frameworkReferenceGroups.Length);
                                            var nearestFramework = NuGetFrameworkUtility.GetNearest(frameworkReferenceGroups, nugetFramefork);
                                            //Debug.WriteLine(nearestFramework.TargetFramework.GetShortFolderName());
                                        }

                                        return new KeyValuePair<ProjectPoco, IEnumerable<string>>(x, new string[0]);
                                    });
                        }
                    })
                    .SelectMany(x => x);
        }

        internal static FrameworkSpecificGroup GetMostCompatibleGroup(NuGetFramework projectTargetFramework, FrameworkSpecificGroup[] itemGroups)
        {
            var frameworkReducer = new FrameworkReducer();
            var mostCompatibleFramework = frameworkReducer.GetNearest(projectTargetFramework, itemGroups.Select(x => x.TargetFramework));
            if (mostCompatibleFramework == null)
                return null;

            var frameworkSpecificGroup = itemGroups.FirstOrDefault(x => x.TargetFramework.Equals(mostCompatibleFramework));
            return IsValid(frameworkSpecificGroup) ? frameworkSpecificGroup : null;
        }

        internal static bool IsValid(FrameworkSpecificGroup frameworkSpecificGroup)
        {
            if (frameworkSpecificGroup == null)
                return false;
            if (!frameworkSpecificGroup.HasEmptyFolder && !frameworkSpecificGroup.Items.Any())
                return !frameworkSpecificGroup.TargetFramework.Equals(NuGetFramework.AnyFramework);
            return true;
        }

        private static IDictionary<ProjectPoco, IEnumerable<string>> ComposeRedirectsVsReferencesErrors(IList<ProjectPoco> projects)
        {
            var appConfigParser = AppConfigParser.Instance;
            return projects.Where(x => x.AppConfigPath != null)
                           .ToDictionary(
                               x => x,
                               x =>
                                   {
                                       var bindings = appConfigParser.ReadBindings(x.AppConfigPath);
                                       var projectReferences = x.References.Select(y => y as ProjectParser.Structure.ProjectReference)
                                                                .Where(y => y != null)
                                                                .Select(y => y.Name);
                                       var references = x.References.Select(y => y as Reference)
                                                         .Where(y => y != null)
                                                         .ToArray();
                                       var dllReferences = references.Select(y => Path.GetFileNameWithoutExtension(y.HintPath))
                                                                     .Concat(references.Select(y => Path.GetFileNameWithoutExtension(y.HintPath)))
                                                                     .Where(y => y != null);
                                       var referenceNames = projectReferences.Concat(dllReferences);

                                       return bindings.Where(y => referenceNames.FirstOrDefault(z => string.Equals(z, y.Name)) == null)
                                                      .Select(y => $"config file has an assembly binding redirect to [{y}] but project doesn't reference lib with the same name");
                                   })
                           .Where(x => x.Value.Any())
                           .ToDictionary();
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
            var appConfigParser = AppConfigParser.Instance;
            var projectBindings = projects.Where(x => x.AppConfigPath != null)
                                          .ToDictionary(
                                              x => x,
                                              x => appConfigParser.ReadBindings(x.AppConfigPath));

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