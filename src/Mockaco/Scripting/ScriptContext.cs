using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Mockaco.Routing;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;

namespace Mockaco
{
    public class ScriptContext : IScriptContext
    {
        public Faker Faker { get; }

        public ScriptContextRequest Request { get; set; }

        public ScriptContextResponse Response { get; set; }

        public ScriptContext()
        {
            Faker = new Faker("pt_BR"); // TODO Localize based on the request

            Request = new ScriptContextRequest(
                default,
                new PermissiveDictionary<string, string>(),
                new PermissiveDictionary<string, string>(),
                new PermissiveDictionary<string, string>(),
                new JObject());

            Response = new ScriptContextResponse(
                new PermissiveDictionary<string, string>(),
                new JObject());
        }

        public void AttachHttpContext(HttpContext httpContext, Route route)
        {
            Request = new ScriptContextRequest(
                 url: httpContext.Request.GetUri(),
                 route: httpContext.Request.GetRouteData(route).ToPermissiveDictionary(k => k.Key, v => v.Value.ToString()),
                 query: httpContext.Request.Query.ToPermissiveDictionary(k => k.Key, v => v.Value.ToString()),
                 header: httpContext.Request.Headers.ToPermissiveDictionary(k => k.Key, v => v.Value.ToString()),
                 body: ParseJsonBody(httpContext.Request));
        }

        public void AttachResponse(IHeaderDictionary headers, JContainer body)
        {
            Response = new ScriptContextResponse(
               headers.ToPermissiveDictionary(k => k.Key, v => v.Value.ToString()),
               body);
        }

        private static JObject ParseJsonBody(HttpRequest httpRequest)
        {
            if (httpRequest.ContentType != "application/json")
            {
                return new JObject();
            }

            httpRequest.EnableRewind();

            using (var reader = new StreamReader(httpRequest.Body))
            {
                var json = reader.ReadToEnd();

                if (!string.IsNullOrWhiteSpace(json))
                {
                    return JObject.Parse(json);
                }

                httpRequest.Body.Seek(0, SeekOrigin.Begin);
            }

            return new JObject();
        }
    }
}