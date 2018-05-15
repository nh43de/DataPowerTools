using System.Collections.Generic;

namespace DataPowerTools.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue result;
            return dictionary.TryGetValue(key, out result) ? result : default(TValue);
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
            TValue defaultValue)
        {
            TValue result;
            return dictionary.TryGetValue(key, out result) ? result : defaultValue;
        }
    }
}