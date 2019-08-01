using Mockaco;

namespace System.Collections.Generic
{
    public static class StringDictionaryExtensions
    {
        public static StringDictionary ToStringDictionary<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, string> keySelector,
            Func<TSource, string> elementSelector)
        {
            var dictionary = new StringDictionary();

            if (source == null)
            {
                return dictionary;
            }

            foreach (var item in source)
            {
                dictionary.Add(keySelector(item), elementSelector(item));
            }

            return dictionary;
        }
    }
}
