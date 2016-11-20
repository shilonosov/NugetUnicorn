using System.Collections.Generic;
using System.Linq;

using NugetUnicorn.Business.Dto;

using NuGet;

namespace NugetUnicorn.Business
{
    public class PackageInspector
    {
        private readonly INugetLibraryProxy _nugetLibraryProxy;

        private readonly IDictionary<PackageKey, PackageNode> _hashset;

        public PackageInspector(INugetLibraryProxy nugetLibraryProxy)
        {
            _nugetLibraryProxy = nugetLibraryProxy;
            _hashset = new Dictionary<PackageKey, PackageNode>();
        }

        public IEnumerable<PackageNode> InspectPackage(IEnumerable<PackageKey> key)
        {
            return key.SelectMany(GetPackageDto)
                      .Select(
                          x =>
                              {
                                  var packageKey = x.Key;

                                  if (_hashset.ContainsKey(packageKey))
                                  {
                                      return _hashset[packageKey];
                                  }

                                  var node = new PackageNode(x);
                                  _hashset.Add(packageKey, node);
                                  var packageDependency = InspectDependency(x);

                                  node.AddAll(packageDependency);
                                  return node;
                              });
        }

        private IEnumerable<PackageDto> GetPackageDto(PackageKey key)
        {
            if (string.IsNullOrEmpty(key.Version))
            {
                return _nugetLibraryProxy.GetById(key.Id);
            }

            var package = _nugetLibraryProxy.GetByKey(key);
            if (package == null)
            {
                return new PackageDto[0];
            }
            return new[] { package };
        }

        private IEnumerable<PackageNode> InspectDependency(PackageDto package)
        {
            if (package.Dependencies == null)
            {
                return new PackageNode[0];
            }

            return package.Dependencies
                          .SelectMany(
                              x =>
                                  {
                                      var packageId = x.Id;
                                      return _nugetLibraryProxy.GetById(packageId)
                                                               .Where(y => x.HasVersionRestriction && x.VersionSpec.Satisfies(y.SemanticVersion))
                                                               .SelectMany(
                                                                   y =>
                                                                       {
                                                                           var packageVersion = y.Key.Version;
                                                                           return InspectPackage(new[] { new PackageKey(packageId, packageVersion) });
                                                                       });
                                  });
        }
    }
}