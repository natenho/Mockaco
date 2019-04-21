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

        public new void Add(TKey key, TValue value)
        {
            Replace(key, value);
        }

        public void Replace(TKey key, TValue value)
        {
            if (ContainsKey(key))
            {
                Remove(key);
            }

            base.Add(key, value);
        }
    }
}