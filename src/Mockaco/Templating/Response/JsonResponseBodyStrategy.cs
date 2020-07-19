using Newtonsoft.Json;

namespace Mockaco
{
    public class JsonResponseBodyStrategy : StringResponseBodyStrategy
    {
        public override bool CanHandle(ResponseTemplate responseTemplate)
        {
            responseTemplate.Headers.TryGetValue(HttpHeaders.ContentType, out var contentType);

            return contentType == null || contentType == HttpContentTypes.ApplicationJson;
        }

        public override string GetResponseBodyStringFromTemplate(ResponseTemplate responseTemplate)
        {
            var formatting = responseTemplate.Indented.GetValueOrDefault(true) ? Formatting.Indented : default;

            return responseTemplate.Body?.ToString(formatting);
        }
    }
}
