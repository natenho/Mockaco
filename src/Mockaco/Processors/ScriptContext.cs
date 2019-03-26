using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mockaco
{
    public class ScriptContext
    {
        public Uri Url { get; }

        public Faker Faker { get; }

        public IReadOnlyDictionary<string, string> Route { get; } // TODO Avoid throwing exception for inexistent keys

        public IReadOnlyDictionary<string, string> Query { get; } // TODO Avoid throwing exception for inexistent keys

        public IReadOnlyDictionary<string, string> Header { get; } // TODO Avoid throwing exception for inexistent keys

        public JObject Body { get; } // TODO Fix the need to do a ToString() to get the value 
        // TODO Improve error handling when the Body item is not found

        public ScriptContext()
        {
            Faker = new Faker("pt_BR"); // TODO Localize based on the request
            Url = default(Uri);
            Route = new Dictionary<string, string>();
            Query = new Dictionary<string, string>();
            Header = new Dictionary<string, string>();
        }

        public ScriptContext(HttpContext httpContext)
            : this()
        {
            Url = httpContext.Request.GetUri();
            Route = httpContext.GetRouteData()
                .Values.ToDictionary(k => k.Key, v => v.Value.ToString());
            Query = httpContext.Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
            Header = httpContext.Request.Headers.ToDictionary(k => k.Key, v => v.Value.ToString());
            Body = ParseJsonBody(httpContext.Request);
        }

        private static JObject ParseJsonBody(HttpRequest httpRequest)
        {
            if (httpRequest.ContentType != "application/json")
            {
                return default(JObject);
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

            return default(JObject);
        }
    }
}