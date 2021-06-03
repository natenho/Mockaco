
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;


namespace Mockaco
{
    public class JsonRequestBodyStrategy: IRequestBodyStrategy
    {
        public bool CanHandle(HttpRequest httpRequest)
        {
            return httpRequest.HasFormContentType;
        }

        public async Task<JToken> ReadBodyAsJson(HttpRequest httpRequest)
        {
            var body = await httpRequest.ReadBodyStream();

            if (string.IsNullOrWhiteSpace(body))
            {
                return new JObject();
            }

            return JToken.Parse(body);
        }
    }
}
