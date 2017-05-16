using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;

using Microsoft.Build.Evaluation;

using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.ReferenceType;
using NugetUnicorn.Business.SourcesParser.ProjectParser;

using NuGet.Frameworks;
using NuGet.Packaging;

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
                                       return FindMissingNugetReferences(solutionPath, x, projectReferences);
                                   })
                           .SelectMany(x => x);
        }

        private static IEnumerable<KeyValuePair<ProjectPoco, IEnumerable<string>>> FindMissingNugetReferences(
            string solutionPath,
            ProjectPoco projectPoco,
            IEnumerable<ReferenceInformation> projectReferences)
        {
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

                return pcr.GetPackages()
                          .Select(
                              y =>
                                  {
                                      var packageIdentity = y.PackageIdentity;
                                      var packagePathResolver =
                                          new PackagePathResolver(Path.Combine(solutionPath, "packages"));
                                      var packageDirectory = packagePathResolver.GetInstallPath(packageIdentity);
                                      var packageFileName = packagePathResolver.GetPackageFileName(packageIdentity);

                                      var packagePath = Path.Combine(packageDirectory, packageFileName);
                                      using (var nupkgStream = File.Open(packagePath, FileMode.Open))
                                      {
                                          var nupkgReader = new PackageArchiveReader(nupkgStream);
                                          var referenceItems = nupkgReader.GetReferenceItems()
                                                                          .ToArray();

                                          var fn = new FrameworkName(frameworkName);

                                          var nugetFramefork = new NuGetFramework(fn.Identifier, fn.Version, fn.Profile);
                                          var mostCompatibleGroup = GetMostCompatibleGroup(nugetFramefork, referenceItems);
                                          var items = nupkgReader.GetReferenceItems()
                                                                 .Where(z => z.TargetFramework == mostCompatibleGroup.TargetFramework)
                                                                 .SelectMany(z => z.Items)
                                                                 .ToArray();

                                          if (items.Any())
                                          {
                                              var missingReferences =
                                                  items
                                                      .Where(
                                                          z => projectReferences.Any(
                                                              xx => string.Equals(xx.AssemblyName, z)))
                                                      .Select(
                                                          z =>
                                                              $"nuget lib folder has dll [{z}], but project doesn't reference it. consier reinstall package [{packageIdentity}]");

                                              return new KeyValuePair<ProjectPoco, IEnumerable<string>>(
                                                  projectPoco,
                                                  missingReferences);
                                          }
                                          return new KeyValuePair<ProjectPoco, IEnumerable<string>>(projectPoco, new string[0]);
                                      }
                                  });
            }
        }

        private static string GetFrameworkIdentifier(IProjectPoco x)
        {
            using (var pc = new ProjectCollection())
            {
                pc.LoadProject(x.ProjectFilePath.FullPath);
                var result = pc.GetLoadedProjects(x.ProjectFilePath.FullPath)
                               .First()
                               .GetPropertyValue(TARGET_FRAMEWORK_MONIKER_NAME);
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