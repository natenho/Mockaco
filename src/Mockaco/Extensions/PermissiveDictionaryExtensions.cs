using Mockaco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace System.Collections.Generic
{
    public static class PermissiveDictionaryExtensions
    {
        public static PermissiveDictionary<TKey, TElement> ToPermissiveDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            Func<TSource, TElement> elementSelector)
        {
            var dictionary = new PermissiveDictionary<TKey, TElement>();

            if(source == null)
            {
                return dictionary;
            }

            foreach(var item in source)
            {                
                dictionary.Add(keySelector(item), elementSelector(item));
            }

            return dictionary;
        }
    }
}
