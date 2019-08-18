using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Mockaco
{
    public class FormRequestBodyStrategy : IRequestBodyStrategy
    {
        public bool CanHandle(HttpRequest httpRequest)
        {
            return httpRequest.HasFormContentType;
        }

        public JObject ReadBodyAsJson(HttpRequest httpRequest)
        {
            return JObject.FromObject(httpRequest.Form.ToDictionary(f => f.Key, f => f.Value.ToString()));
        }
    }
}
