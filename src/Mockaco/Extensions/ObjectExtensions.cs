using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace System
{
    public static class ObjectExtensions
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = new JsonConverter[] { new StringEnumConverter { CamelCaseText = true } }
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
    }
}