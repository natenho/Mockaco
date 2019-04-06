using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;

namespace Mockaco
{

    public class ScriptContext
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
                new PermissiveJraw(string.Empty));

            Response = new ScriptContextResponse(
                new PermissiveDictionary<string, string>(),
                new PermissiveJraw(string.Empty));
        }

        public void AttachHttpContext(HttpContext httpContext)
        {
            Request = new ScriptContextRequest(
                url: httpContext.Request.GetUri(),
                route: httpContext.GetRouteData()?.Values.ToPermissiveDictionary(k => k.Key, v => v.Value.ToString()), //TODO Always null. How to resolve this?
                query: httpContext.Request.Query.ToPermissiveDictionary(k => k.Key, v => v.Value.ToString()),
                header: httpContext.Request.Headers.ToPermissiveDictionary(k => k.Key, v => v.Value.ToString()),
                body: ParseJsonBody(httpContext.Request));
        }

        public void AttachResponse(IHeaderDictionary headers, JRaw body)
        {
            var jsonBody = new PermissiveJraw(string.Empty);

            if (headers.TryGetValue("Content-Type", out var contentType))
            {
                jsonBody = new PermissiveJraw(body);
            }

            Response = new ScriptContextResponse(
               headers.ToPermissiveDictionary(k => k.Key, v => v.Value.ToString()),
               jsonBody);
        }

        private static PermissiveJraw ParseJsonBody(HttpRequest httpRequest)
        {
            if (httpRequest.ContentType != "application/json")
            {
                return new PermissiveJraw(string.Empty);
            }

            httpRequest.EnableRewind();

            using (var reader = new StreamReader(httpRequest.Body))
            {
                var json = reader.ReadToEnd();

                if (!string.IsNullOrWhiteSpace(json))
                {
                    return new PermissiveJraw(json);
                }

                httpRequest.Body.Seek(0, SeekOrigin.Begin);
            }

            return new PermissiveJraw(string.Empty);
        }
    }
}