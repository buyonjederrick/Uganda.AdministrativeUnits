using System;
using System.Collections.Generic;

namespace Uganda.AdministrativeUnits.Internal;

internal static class DictionaryExtensions
{
    public static bool TryAddValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        where TKey : notnull
    {
        if (dictionary.ContainsKey(key))
        {
            return false;
        }

        dictionary.Add(key, value);
        return true;
    }

    public static TValue? GetValueOrDefaultCompat<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        where TKey : notnull
        where TValue : class
    {
        return dictionary.TryGetValue(key, out TValue? value) ? value : null;
    }
}
