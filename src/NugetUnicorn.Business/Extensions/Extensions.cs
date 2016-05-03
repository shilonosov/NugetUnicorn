using System;
using System.Collections.Generic;
using System.Linq;

namespace NugetUnicorn.Business.Extensions
{
    public static class Extensions
    {
        public static void IfTrue(this bool value, Action ifTrueAction)
        {
            if (value)
            {
                ifTrueAction();
            }
        }

        public static void ForEachItem<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        }

        public static IEnumerable<T> Do<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            var list = enumerable.ToList();
            list.ForEachItem(action);
            return list;
        }

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