using System.Collections.Generic;
using System.Linq;

using NuGet;

namespace NugetUnicorn.Business.Dto
{
    public class PackageDto
    {
        public SemanticVersion SemanticVersion { get; set; }

        public IList<PackageDependencyDto> Dependencies { get; set; }

        public bool IsReleaseVersion { get; set; }

        public PackageKey Key { get; set; }

        public PackageDto()
        {
        }

        public PackageDto(IPackage package)
        {
            Key = new PackageKey(package.Id, package.Version.ToString());
            IsReleaseVersion = package.IsReleaseVersion();
            Dependencies = GetPackageDependencyDtos(package);
            SemanticVersion = package.Version;
        }

        private static IList<PackageDependencyDto> GetPackageDependencyDtos(IPackage package)
        {
            if (package.DependencySets == null)
            {
                return new List<PackageDependencyDto>();
            }

            return package.DependencySets
                          .SelectMany(x => x.Dependencies)
                          .Where(x => x != null)
                          .Select(x => new PackageDependencyDto(x))
                          .ToList();
        }
    }
}