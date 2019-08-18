using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Linq;

namespace System
{
    public static class ObjectExtensions
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = new JsonConverter[] { new StringEnumConverter { NamingStrategy = new CamelCaseNamingStrategy()} },
            NullValueHandling = NullValueHandling.Ignore
        };

        public static string ToJson<T>(this T param)
            where T : class
        {
            if (param == null)
            {
                return string.Empty;
            }

            try
            {
                return JsonConvert.SerializeObject(param, JsonSerializerSettings);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static bool IsAnyOf<T>(this T item, params T[] possibleItems)
        {
            return possibleItems.Contains(item);
        }
    }
}