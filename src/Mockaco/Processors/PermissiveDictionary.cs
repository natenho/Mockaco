using System.Collections.Generic;

namespace Mockaco
{
    public class PermissiveDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        public new TValue this[TKey key]
        {
            get
            {
                if (TryGetValue(key, out TValue value))
                {
                    return value;
                }
                else
                {
                    return default(TValue);
                }
            }
            set
            {
                base[key] = value;
            }
        }
    }
}