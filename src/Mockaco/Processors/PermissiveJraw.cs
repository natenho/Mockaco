using Newtonsoft.Json.Linq;
using System;

namespace Mockaco
{
    public class PermissiveJraw : JRaw
    {
        public PermissiveJraw(JRaw other) : base(other)
        {
        }

        public PermissiveJraw(object rawJson) : base(rawJson)
        {
        }

        public override JToken this[object key]
        {
            get
            {
                if(!HasValues)
                {
                    return new JValue(string.Empty);
                }

                try
                {
                    return base[key];
                }
                catch (InvalidOperationException)
                {
                    return new JValue(string.Empty);
                }
            }
            set
            {
                base[key] = value;
            }
        }
    }
}