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

        public ScriptContext(HttpContext httpContext)
        {
            Url = httpContext.Request.GetUri();
            
            Faker = new Faker("pt_BR"); // TODO Localize based on the request
            
            Route = httpContext.GetRouteData()
                .Values.ToDictionary(k => k.Key, v => v.Value.ToString());

            Query = httpContext.Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());

            Header = httpContext.Request.Headers.ToDictionary(k => k.Key, v => v.Value.ToString());
            
            httpContext.Request.EnableRewind();
            using (var reader = new StreamReader(httpContext.Request.Body))
            {
                var json = reader.ReadToEnd();

                if (!string.IsNullOrWhiteSpace(json))
                {
                    Body = JObject.Parse(json);
                }

                httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
            }
        }
    }
}