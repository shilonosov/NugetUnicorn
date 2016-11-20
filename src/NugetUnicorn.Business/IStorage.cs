using System;
using System.Collections.Generic;

namespace NugetUnicorn.Business
{
    public interface IStorage<TValue>
    {
        bool HasKey(PackageKey key);

        bool HasId(string packageId);

        StorageEntity<TValue> GetByKey(PackageKey key);

        StorageEntity<IEnumerable<TValue>> GetById(string packageId);

        StorageEntity<TValue> Put(PackageKey key, TValue value);
    }

    public struct StorageEntity<TValue>
    {
        public DateTime LastModified { get; private set; }

        public TValue Value { get; private set; }

        public StorageEntity(DateTime lastModifiedDateTime, TValue value)
        {
            LastModified = lastModifiedDateTime;
            Value = value;
        }
    }
}