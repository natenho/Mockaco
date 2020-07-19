using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace Mockaco
{
    public class FormRequestBodyStrategy : IRequestBodyStrategy
    {
        public bool CanHandle(HttpRequest httpRequest)
        {
            return httpRequest.HasFormContentType;
        }

        public Task<JObject> ReadBodyAsJson(HttpRequest httpRequest)
        {
            return Task.FromResult(JObject.FromObject(httpRequest.Form.ToDictionary(f => f.Key, f => f.Value.ToString())));
        }
    }
}
