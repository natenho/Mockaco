using Mockaco.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Linq;

namespace System
{
    internal static class ObjectExtensions
    {
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = [
                new StringEnumConverter { NamingStrategy = new CamelCaseNamingStrategy()}, 
                new SimpleExceptionConverter() 
            ],
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
                return JsonConvert.SerializeObject(param, _jsonSerializerSettings);
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