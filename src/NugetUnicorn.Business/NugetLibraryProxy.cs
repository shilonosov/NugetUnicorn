using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using NugetUnicorn.Business.Dto;
using NugetUnicorn.Business.Extensions;

using NuGet;

namespace NugetUnicorn.Business
{
    public class NugetLibraryProxy : INugetLibraryProxy
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromDays(7);

        private readonly IStorage<PackageDto> _storage;

        private readonly IPackageRepository _packageRepository;

        public NugetLibraryProxy(IStorage<PackageDto> storage, IPackageRepository packageRepository)
        {
            _storage = storage;
            _packageRepository = packageRepository;
        }

        public IEnumerable<PackageDto> GetById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new PackageDto[0];
            }
            var result = _storage.GetById(id);
            if (IsOutdated(result))
            {
                Debug.WriteLine($"FindPackagesById {id}");
                return _packageRepository.FindPackagesById(id)
                                         .Where(x => x.IsReleaseVersion())
                                         .Select(x => new PackageDto(x))
                                         .Do(x => _storage.Put(x.Key, x));
            }
            return result.Value;
        }

        public PackageDto GetByKey(PackageKey key)
        {
            if (!_storage.HasKey(key))
            {
                return RetrieveAndSaveInternal(key);
            }

            var storageEntity = _storage.GetByKey(key);
            if (IsOutdated(storageEntity))
            {
                return RetrieveAndSaveInternal(key);
            }

            return storageEntity.Value;
        }

        private static bool IsOutdated<T>(StorageEntity<T> storageEntity)
        {
            var storeTime = DateTime.Now - storageEntity.LastModified;
            var isOutdated = storeTime > Timeout;
            return isOutdated;
        }

        private PackageDto RetrieveAndSaveInternal(PackageKey key)
        {
            Debug.WriteLine($"FindPackage {key}");
            var package = _packageRepository.FindPackage(key.Id, new SemanticVersion(key.Version));
            if (package == null)
            {
                return null;
            }
            var packageDto = new PackageDto(package);
            _storage.Put(key, packageDto);
            return packageDto;
        }

        public VersionSpec GetVersionRange(IEnumerable<PackageKey> packageKeys)
        {
            var keys = packageKeys as PackageKey[] ?? packageKeys.ToArray();
            var orderedByVersion = keys.Select(x => new Tuple<PackageKey, SemanticVersion>(x, new SemanticVersion(x.Version)))
                                       .OrderBy(x => x.Item2);
            var existingVersions = GetById(keys.First().Id).Select(x => new Tuple<PackageKey, SemanticVersion>(x.Key, x.SemanticVersion))
                                                           .OrderBy(x => x.Item2)
                                                           .AsEnumerable();

            var firstKey = orderedByVersion.First();
            existingVersions = existingVersions.SkipWhile(x => x.Item2 != firstKey.Item2);

            while (true)
            {
            }
        }
    }
}