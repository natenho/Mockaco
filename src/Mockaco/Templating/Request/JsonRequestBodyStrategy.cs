using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace Mockaco
{
    public class JsonRequestBodyStrategy: IRequestBodyStrategy
    {
        public bool CanHandle(HttpRequest httpRequest)
        {
            return httpRequest.HasJsonContentType();
        }

        public JObject ReadBodyAsJson(HttpRequest httpRequest)
        {
            var body = httpRequest.ReadBodyStream();

            if (string.IsNullOrWhiteSpace(body))
            {
                return new JObject();
            }

            return JObject.Parse(body);
        }
    }
}
