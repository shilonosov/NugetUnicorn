using NuGet;
using PackageDependency = NuGet.PackageDependency;

namespace NugetUnicorn.Business.Dto
{
    public class PackageDependencyDto
    {
        public bool HasVersionRestriction { get; set; }

        public string Id { get; set; }

        public VersionSpec VersionSpec { get; set; }

        public PackageDependencyDto(PackageDependency packageDependency)
        {
            Id = packageDependency.Id;
            var versionSpec = packageDependency.VersionSpec;
            if (versionSpec == null)
            {
                return;
            }

            HasVersionRestriction = true;
            VersionSpec = new VersionSpec
                              {
                                  MinVersion = versionSpec.MinVersion,
                                  MaxVersion = versionSpec.MaxVersion,
                                  IsMinInclusive = versionSpec.IsMinInclusive,
                                  IsMaxInclusive = versionSpec.IsMaxInclusive
                              };
        }

        public PackageDependencyDto()
        {
        }
    }
}