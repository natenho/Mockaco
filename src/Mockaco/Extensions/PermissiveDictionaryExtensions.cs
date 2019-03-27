using Mockaco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace System.Collections.Generic
{
    public static class PermissiveDictionaryExtensions
    {
        public static PermissiveDictionary<TKey, TElement> ToPermissiveDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return (PermissiveDictionary<TKey, TElement>)Enumerable.ToDictionary(source, keySelector, elementSelector);
        }
    }
}
