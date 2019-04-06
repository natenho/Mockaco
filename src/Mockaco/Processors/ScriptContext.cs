using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace Mockaco
{

    public class ScriptContext
    {
        public Uri Url { get; }

        public Faker Faker { get; }

        public IReadOnlyDictionary<string, string> Route { get; }

        public IReadOnlyDictionary<string, string> Query { get; }

        public IReadOnlyDictionary<string, string> Header { get; }

        public PermissiveJraw Body { get; }

        public ScriptContext()
        {
            Faker = new Faker("pt_BR"); // TODO Localize based on the request
            Url = default;
            Route = new PermissiveDictionary<string, string>();
            Query = new PermissiveDictionary<string, string>();
            Header = new PermissiveDictionary<string, string>();
            Body = new PermissiveJraw(string.Empty);
        }

        public ScriptContext(HttpContext httpContext)
            : this()
        {
            Url = httpContext.Request.GetUri();
            Route = httpContext.GetRouteData()?.Values.ToPermissiveDictionary(k => k.Key, v => v.Value.ToString());//TODO Always null. How to resolve this?
            Query = httpContext.Request.Query.ToPermissiveDictionary(k => k.Key, v => v.Value.ToString());
            Header = httpContext.Request.Headers.ToPermissiveDictionary(k => k.Key, v => v.Value.ToString());
            Body = ParseJsonBody(httpContext.Request);
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