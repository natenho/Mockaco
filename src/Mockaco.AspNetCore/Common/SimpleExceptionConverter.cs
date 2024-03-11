namespace Mockaco.Common
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;

    internal class SimpleExceptionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Exception).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Deserializing exceptions is not supported.");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var exception = value as Exception;
            if (exception == null)
            {
                serializer.Serialize(writer, null);
                return;
            }

            var obj = new JObject
            {
                ["Type"] = exception.GetType().Name,
                ["Message"] = exception.Message,

            };

            if (exception.Data.Count > 0)
            {
                obj["Data"] = JToken.FromObject(exception.Data, serializer);
            }

            if (exception.InnerException != null)
            {
                obj["InnerException"] = JToken.FromObject(exception.InnerException, serializer);
            }

            obj.WriteTo(writer);
        }
    }

}
