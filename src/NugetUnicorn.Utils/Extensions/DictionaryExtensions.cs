using System;
using System.Collections.Generic;
using System.Linq;

namespace NugetUnicorn.Utils.Extensions
{
    public static class DictionaryExtensions
    {
        public static IDictionary<TKey, TNewValue> Transform<TKey, TValue, TNewValue>(this IDictionary<TKey, TValue> dictionary, Func<TValue, TNewValue> transformationSelector)
        {
            return dictionary.ToDictionary(x => x.Key, x => transformationSelector(x.Value));
        }

        public static IDictionary<TKey, TNewValue> Transform<TKey, TValue, TNewValue>(this IDictionary<TKey, TValue> dictionary,
                                                                                      Func<TKey, TValue, TNewValue> transformationSelector)
        {
            return dictionary.ToDictionary(x => x.Key, x => transformationSelector(x.Key, x.Value));
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary.TryGetValue(key, out TValue result))
            {
                return result;
            }
            return default(TValue);
        }
    }
}