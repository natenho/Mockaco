using Newtonsoft.Json;

namespace Mockaco
{
    public class JsonResponseBodyStrategy : IResponseBodyStrategy 
    {
        public bool CanHandle(ResponseTemplate responseTemplate)
        {
            responseTemplate.Headers.TryGetValue(HttpHeaders.ContentType, out var contentType);

            return contentType == null || contentType == HttpContentTypes.ApplicationJson;
        }

        public string GetResponseBodyFromTemplate(ResponseTemplate responseTemplate)
        {
            var formatting = responseTemplate.Indented.GetValueOrDefault(true) ? Formatting.Indented : default;

            return responseTemplate.Body?.ToString(formatting);
        }
    }
}
