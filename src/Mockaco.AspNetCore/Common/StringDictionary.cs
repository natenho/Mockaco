using System.Collections.Generic;

namespace Mockaco
{
    public class StringDictionary : Dictionary<string, string>, IReadOnlyDictionary<string, string>
    {
        public new string this[string key]
        {
            get
            {
                if (TryGetValue(key, out string value))
                {
                    return value;
                }
                else
                {
                    return default;
                }
            }
            set
            {
                base[key] = value;
            }
        }

        public new void Add(string key, string value)
        {
            Replace(key, value);
        }

        public void Replace(string key, string value)
        {
            if (ContainsKey(key))
            {
                Remove(key);
            }

            base.Add(key, value);
        }
    }
}