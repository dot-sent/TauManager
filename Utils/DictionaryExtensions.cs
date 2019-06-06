using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TauManager.Utils
{
    public static class DictionaryExtensions
    {
        public static TValue SingleOrGiven<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }
            return defaultValue;
        }

        public static async Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TInput, TKey, TValue>(
            this IEnumerable<TInput> enumerable,
            Func<TInput, TKey> syncKeySelector,
            Func<TInput, Task<TValue>> asyncValueSelector)
        {
            Dictionary<TKey,TValue> dictionary = new Dictionary<TKey, TValue>();

            foreach (var item in enumerable)
            {
                var key = syncKeySelector(item);

                var value = await asyncValueSelector(item);

                dictionary.Add(key,value);
            }

            return dictionary;
        }
    }
}