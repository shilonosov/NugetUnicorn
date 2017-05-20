using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;

using Microsoft.Build.Evaluation;

using NugetUnicorn.Business.Extensions;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.Metadata;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.ReferenceType;
using NugetUnicorn.Business.SourcesParser.ProjectParser;

using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;

using static System.String;

namespace NugetUnicorn.Business.SourcesParser
{
    public class InconsistentNugetPackagesAnalyzer
    {
        private const string TARGET_FRAMEWORK_MONIKER_NAME = "TargetFrameworkMoniker";

        public static IEnumerable<KeyValuePair<ProjectPoco, IEnumerable<string>>>
            ComposeInconsistentNugetPackageReferences(
                string solutionPath,
                IEnumerable<ProjectPoco> projects,
                IDictionary<ProjectPoco, IEnumerable<ReferenceInformation>> referencesByProjects,
                IDictionary<ProjectPoco, IEnumerable<ReferenceMetadataBase>> referenceMetadatas)
        {
            var allFileReferences = referenceMetadatas.Transform(x => x.OfType<ExistingReferenceMetadataBase>());
            return projects.Select(
                               x =>
                                   {
                                       var projectReferences = referencesByProjects[x];
                                       if (IsNullOrEmpty(x.PackagesConfigPath))
                                       {
                                           return new[]
                                                      {
                                                          new KeyValuePair<ProjectPoco, IEnumerable<string>>(x, new string[0])
                                                      };
                                       }
                                       var refeferenceMetadatas = allFileReferences.GetOrDefault(x);
                                       return FindMissingNugetReferences(solutionPath, x, projectReferences, refeferenceMetadatas);
                                   })
                           .SelectMany(x => x);
        }

        private static IEnumerable<KeyValuePair<ProjectPoco, IEnumerable<string>>> FindMissingNugetReferences(
            string solutionPath,
            ProjectPoco projectPoco,
            IEnumerable<ReferenceInformation> projectReferences,
            IEnumerable<ExistingReferenceMetadataBase> nugetRefeferenceMetadatas)
        {
            var projectReferencessArray = projectReferences.ToArray();
            var packagesRootDirectory = Path.GetFullPath(Path.Combine(solutionPath, "packages"));

            using (var pcs = File.Open(projectPoco.PackagesConfigPath, FileMode.Open))
            {
                string frameworkName;
                try
                {
                    frameworkName = GetFrameworkIdentifier(projectPoco);
                }
                catch (Exception e)
                {
                    return new[]
                               {
                                   new KeyValuePair<ProjectPoco, IEnumerable<string>>(
                                       projectPoco,
                                       new[] { $"error loading project: {e.Message}" })
                               };
                }
                var pcr = new PackagesConfigReader(pcs);

                var packages = pcr.GetPackages()
                                  .ToArray();
                var packageItems = packages.Select(
                                               x =>
                                                   {
                                                       var packageIdentity = x.PackageIdentity;
                                                       var nugetPackageItems = GetNugetPackageItems(packageIdentity, frameworkName, packagesRootDirectory);
                                                       return new KeyValuePair<PackageIdentity, string[]>(packageIdentity, nugetPackageItems);
                                                   })
                                           .ToArray();
                var allPackageItems = packageItems.SelectMany(
                                                      x => x.Value.Select(
                                                          y => Path.GetFullPath(Path.Combine(packagesRootDirectory, $"{x.Key.Id}.{x.Key.Version}", y.Replace("/", "\\")))))
                                                  .ToArray();

                var referencesWithoutPackage = projectReferencessArray.Where(x => !string.IsNullOrEmpty(x.FullPath))
                                                                      .Where(x => x.FullPath.StartsWith(packagesRootDirectory))
                                                                      .Where(x => !allPackageItems.Contains(x.FullPath))
                                                                      .Select(
                                                                          x =>
                                                                              $"reference to a file inside packages folder [{x.FullPath} {x.AssemblyName}, {x.Version}] without entry in packages.config file. consider to install it from nuget");

                var missingNugetReferences = packageItems.Select(
                                                             y =>
                                                                 {
                                                                     var packageIdentity = y.Key;
                                                                     var nugetPackageItems = y.Value;
                                                                     var missingReferencesToNugetPackageDlls = nugetPackageItems
                                                                         .Where(x => projectReferencessArray.Any(z => string.Equals(z.AssemblyName, x)))
                                                                         .Select(
                                                                             x =>
                                                                                 $"nuget lib folder has dll [{x}], but project doesn't reference it. consier reinstall package [{packageIdentity}]");

                                                                     return new KeyValuePair<ProjectPoco, IEnumerable<string>>(projectPoco, missingReferencesToNugetPackageDlls);
                                                                 })
                                                         .ToList();
                missingNugetReferences.Add(new KeyValuePair<ProjectPoco, IEnumerable<string>>(projectPoco, referencesWithoutPackage));
                return missingNugetReferences;
            }
        }

        private static string[] GetNugetPackageItems(PackageIdentity packageIdentity, string frameworkName, string packagesRootDirectory)
        {
            var packagePathResolver = new PackagePathResolver(packagesRootDirectory);
            var packageDirectory = packagePathResolver.GetInstallPath(packageIdentity);
            var packageFileName = packagePathResolver.GetPackageFileName(packageIdentity);

            var packagePath = Path.Combine(packageDirectory, packageFileName);
            string[] items;
            using (var nupkgStream = File.Open(packagePath, FileMode.Open))
            {
                var nupkgReader = new PackageArchiveReader(nupkgStream);
                var referenceItems = nupkgReader.GetReferenceItems()
                                                .ToArray();

                var fn = new FrameworkName(frameworkName);

                var nugetFramefork = new NuGetFramework(fn.Identifier, fn.Version, fn.Profile);
                var mostCompatibleGroup = GetMostCompatibleGroup(nugetFramefork, referenceItems);
                items = nupkgReader.GetReferenceItems()
                                   .Where(z => z.TargetFramework == mostCompatibleGroup.TargetFramework)
                                   .SelectMany(z => z.Items)
                                   .ToArray();
            }
            return items;
        }

        private static string GetFrameworkIdentifier(IProjectPoco x)
        {
            using (var pc = new ProjectCollection())
            {
                pc.LoadProject(x.ProjectFilePath.FullPath);
                var project = pc.GetLoadedProjects(x.ProjectFilePath.FullPath)
                                .First();
                var result = project.GetPropertyValue(TARGET_FRAMEWORK_MONIKER_NAME);
                pc.UnloadAllProjects();
                return result;
            }
        }

        private static FrameworkSpecificGroup GetMostCompatibleGroup(NuGetFramework projectTargetFramework,
                                                                     FrameworkSpecificGroup[] itemGroups)
        {
            var frameworkReducer = new FrameworkReducer();
            var mostCompatibleFramework =
                frameworkReducer.GetNearest(projectTargetFramework, itemGroups.Select(x => x.TargetFramework));
            if (mostCompatibleFramework == null)
                return null;

            var frameworkSpecificGroup =
                itemGroups.FirstOrDefault(x => x.TargetFramework.Equals(mostCompatibleFramework));
            return IsValid(frameworkSpecificGroup) ? frameworkSpecificGroup : null;
        }

        private static bool IsValid(FrameworkSpecificGroup frameworkSpecificGroup)
        {
            if (frameworkSpecificGroup == null)
                return false;
            if (!frameworkSpecificGroup.HasEmptyFolder && !frameworkSpecificGroup.Items.Any())
                return !frameworkSpecificGroup.TargetFramework.Equals(NuGetFramework.AnyFramework);
            return true;
        }
    }
}