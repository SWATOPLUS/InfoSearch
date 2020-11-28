using System;
using System.Collections.Generic;
using System.Linq;

namespace DuplicateFinder.Core
{
    public static class DictionaryExtensions
    {
        public static Dictionary<TKey, TValueOut> MapValues<TKey, TValueIn, TValueOut>(
            this IReadOnlyDictionary<TKey, TValueIn> dict,
            Func<TValueIn, TValueOut> mapFunc)
        {
            return dict.ToDictionary(x => x.Key, x => mapFunc(x.Value));
        }

        public static Dictionary<TKey, TValue> RemoveKeys<TKey, TValue>(
            this Dictionary<TKey, TValue> dict,
            IEnumerable<TKey> keys)
        {
            var set = keys.ToHashSet(); 

            return dict
                .Where(x => !set.Contains(x.Key))
                .ToDictionary(x => x.Key, x => x.Value);
        }

        public static TValue GetOrDefault<TKey, TValue>(
            this Dictionary<TKey, TValue> dictionary,
            TKey key,
            TValue defaultValue = default)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }

            return defaultValue;
        }
    }
}
