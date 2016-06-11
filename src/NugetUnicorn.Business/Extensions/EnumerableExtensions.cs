using System;
using System.Collections.Generic;
using System.Linq;

using NugetUnicorn.Business.Extensions.EnumerableSwitch;
using NugetUnicorn.Business.FuzzyMatcher.Engine;

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

        public static IEnumerable<ProbabilityMatchMetadata<T>> FindBestMatch<T>(this IEnumerable<T> enumerabe, IProbabilityMatchEngine<T> probabilityMatchEngine)
        {
            return enumerabe.Select(probabilityMatchEngine.FindBestMatch);
        }

        public static IEnumerable<TV> FindBestMatch<T, TV>(this IEnumerable<T> enumerabe, IProbabilityMatchEngine<T> probabilityMatchEngine, double threshold)
            where TV : ProbabilityMatchMetadata<T>
        {
            return enumerabe.Select(probabilityMatchEngine.FindBestMatch)
                            .OfType<TV>()
                            .Where(x => x.Probability > threshold);
        }

        public static IEnumerable<KeyValuePair<TKey, TValue>> ToKeyValueCollection<T, TKey, TValue>(this IEnumerable<T> enumerable,
                                                                                                    Func<T, TKey> keySelector,
                                                                                                    Func<T, TValue> valueSelector)
        {
            return enumerable.Select(x => new KeyValuePair<TKey, TValue>(keySelector(x), valueSelector(x)));
        }

        public static IEnumerable<KeyValuePair<TKey, TNewValue>> Transform<TKey, TValue, TNewValue>(this IEnumerable<KeyValuePair<TKey, TValue>> enumerable,
                                                                                                    Func<TValue, TNewValue> transformationSelector)
        {
            return enumerable.Select(x => new KeyValuePair<TKey, TNewValue>(x.Key, transformationSelector(x.Value)));
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> enumerable, IEnumerable<T> second, Func<T, T, bool> compareFunc)
        {
            return enumerable.Except(second, new LambdaEqualityComparer<T>(compareFunc));
        }

        public static IEnumerable<T> DoIfEmpty<T>(this IEnumerable<T> enumerable, Action ifEmptyAction)
        {
            var hasItems = false;
            foreach (var item in enumerable)
            {
                hasItems = true;
                yield return item;
            }

            if (!hasItems)
            {
                ifEmptyAction();
            }
        }

        public static IDictionary<TKey, IEnumerable<TValue>> Merge<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> first,
                                                                                 IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> second)
        {
            var result = new Dictionary<TKey, IEnumerable<TValue>>();
            var union = first.Union(second);
            foreach (var item in union)
            {
                var thisKey = item.Key;
                var thisValue = item.Value;
                if (result.ContainsKey(thisKey))
                {
                    result[thisKey] = result[thisKey].Union(thisValue);
                }
                else
                {
                    result.Add(thisKey, thisValue);
                }
            }
            return result;
        }
    }

    public class LambdaEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _compareFunc;

        public LambdaEqualityComparer(Func<T, T, bool> compareFunc)
        {
            _compareFunc = compareFunc;
        }

        public bool Equals(T x, T y)
        {
            return _compareFunc(x, y);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}