using System;
using System.Collections.Generic;
using System.Linq;

namespace NugetUnicorn.Business.Extensions
{
    public static class DictionaryExtensions
    {
        public static IDictionary<TKey, TNewValue> Transform<TKey, TValue, TNewValue>(this IDictionary<TKey, TValue> dictionary, Func<TValue, TNewValue> transformationSelector)
        {
            return dictionary.ToDictionary(x => x.Key, x => transformationSelector(x.Value));
        }

        public static IDictionary<TKey, TNewValue> Transform<TKey, TValue, TNewValue>(this IDictionary<TKey, TValue> dictionary, Func<TKey, TValue, TNewValue> transformationSelector)
        {
            return dictionary.ToDictionary(x => x.Key, x => transformationSelector(x.Key, x.Value));
        }
    }
}