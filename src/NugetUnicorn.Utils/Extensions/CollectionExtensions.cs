using System.Collections.Generic;

namespace NugetUnicorn.Utils.Extensions
{
    public static class CollectionExtensions
    {
        public static ICollection<T> AddRange<T>(this ICollection<T> collection, IEnumerable<T> collectionToAdd)
        {
            foreach (var item in collectionToAdd)
            {
                collection.Add(item);
            }
            return collection;
        }
    }
}