using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

namespace NugetUnicorn.Business
{
    public class Storage<TValue> : IStorage<TValue>
    {
        private const string MARKER_TXT = "marker.txt";

        private readonly string _rootFolder;

        private readonly IDictionary<PackageKey, StorageEntity<TValue>> _cache;

        private string ComposeFilePath(PackageKey key)
        {
            return Path.Combine(_rootFolder, key.FullPath);
        }

        private string ComposeDirectoryPath(PackageKey key)
        {
            return ComposeDirectoryPath(key.Id);
        }

        private string ComposeDirectoryPath(string packageId)
        {
            return Path.Combine(_rootFolder, packageId);
        }

        private DateTime GetDirectoryLastModifiedDate(string directoryPath)
        {
            var markerFilePath = ComposeMarkerFilePath(directoryPath);
            if (File.Exists(markerFilePath))
            {
                return File.GetLastWriteTime(markerFilePath);
            }
            return DateTime.MinValue;
        }

        private static string ComposeMarkerFilePath(string directoryPath)
        {
            return Path.Combine(directoryPath, MARKER_TXT);
        }

        public Storage(string rootFolder)
        {
            _rootFolder = rootFolder;
            _cache = new Dictionary<PackageKey, StorageEntity<TValue>>();
        }

        public bool HasKey(PackageKey key)
        {
            if (_cache.ContainsKey(key))
            {
                return true;
            }
            if (File.Exists(ComposeFilePath(key)))
            {
                _cache[key] = Load(key);
                return true;
            }
            return false;
        }

        public bool HasId(string packageId)
        {
            return Directory.Exists(ComposeDirectoryPath(packageId));
        }

        private StorageEntity<TValue> Load(PackageKey key)
        {
            var filePath = ComposeFilePath(key);
            var json = File.ReadAllText(filePath);
            var deserializeObject = JsonConvert.DeserializeObject<TValue>(json);
            var lastModified = File.GetLastWriteTime(filePath);
            return new StorageEntity<TValue>(lastModified, deserializeObject);
        }

        private StorageEntity<TValue> Save(PackageKey key, TValue value)
        {
            var filePath = ComposeFilePath(key);
            var directoryPath = ComposeDirectoryPath(key);
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            var json = JsonConvert.SerializeObject(value);
            File.WriteAllText(filePath, json);
            var lastModifiedTime = File.GetLastWriteTime(filePath);

            UpdateLastWriteDateTimeMarkerFile(directoryPath);

            return new StorageEntity<TValue>(lastModifiedTime, value);
        }

        private void UpdateLastWriteDateTimeMarkerFile(string directoryPath)
        {
            var markerFilePath = ComposeMarkerFilePath(directoryPath);
            if (File.Exists(markerFilePath))
            {
                File.SetLastWriteTime(markerFilePath, DateTime.Now);
            }
            else
            {
                File.Create(markerFilePath)
                    .Dispose();
            }
        }

        public StorageEntity<TValue> GetByKey(PackageKey key)
        {
            return HasKey(key) ? _cache[key] : default(StorageEntity<TValue>);
        }

        public StorageEntity<IEnumerable<TValue>> GetById(string packageId)
        {
            var directoryPath = ComposeDirectoryPath(packageId);
            if (Directory.Exists(directoryPath))
            {
                var lastModified = GetDirectoryLastModifiedDate(directoryPath);
                var storageEntities = Directory.GetFiles(directoryPath, "*." + PackageKey.FileExtension)
                                               .Select(x => GetByKey(new PackageKey(packageId, Path.GetFileNameWithoutExtension(x))))
                                               .Select(x => x.Value);

                return new StorageEntity<IEnumerable<TValue>>(lastModified, storageEntities);
            }
            return new StorageEntity<IEnumerable<TValue>>();
        }

        public StorageEntity<TValue> Put(PackageKey key, TValue value)
        {
            var storageEntity = Save(key, value);
            _cache[key] = storageEntity;
            return storageEntity;
        }
    }
}
