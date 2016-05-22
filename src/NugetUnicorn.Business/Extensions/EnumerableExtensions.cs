using System;
using System.Collections.Generic;
using System.Linq;

using NugetUnicorn.Business.Extensions.EnumerableSwitch;

namespace NugetUnicorn.Business.Extensions
{
    public static class EnumerableExtensions
    {
        public static void ForEachItem<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        }

        public static IEnumerableSwitch<T> Switch<T>(this IEnumerable<T> enumerable)
        {
            return new EnumerableSwitch<T>(enumerable);
        }

        public static IEnumerable<T> Do<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            var list = enumerable.ToList();
            list.ForEachItem(action);
            return list;
        }

        public static IList<T> Do<T>(this IList<T> list, Action<T> action)
        {
            list.ForEachItem(action);
            return list;
        }
    }
}