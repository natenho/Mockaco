using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Mockaco.Routing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mockaco
{
    public class ScriptContext : IScriptContext
    {
        private Uri _uri;
        private PermissiveDictionary<string, string> _queryDictionary = new PermissiveDictionary<string, string>();
        private PermissiveDictionary<string, string> _headersDictionary = new PermissiveDictionary<string, string>();
        private PermissiveDictionary<string, string> _routeDictionary = new PermissiveDictionary<string, string>();
        private JObject _parsedBody;

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

            Response = new ScriptContextResponse(new PermissiveDictionary<string, string>(), new JObject());
        }

        public void AttachHttpContext(HttpContext httpContext)
        {
            _uri = httpContext.Request.GetUri();
            _queryDictionary = httpContext.Request.Query.ToPermissiveDictionary(k => k.Key, v => v.Value.ToString());
            _headersDictionary = httpContext.Request.Headers.ToPermissiveDictionary(k => k.Key, v => v.Value.ToString());
            _parsedBody = ParseBody(httpContext.Request);

            Request = new ScriptContextRequest(url:_uri, route:_routeDictionary, query:_queryDictionary, header:_headersDictionary, body:_parsedBody);
        }

        public void AttachRoute(HttpContext httpContext, Route route)
        {
            _routeDictionary = httpContext.Request.GetRouteData(route)
                .ToPermissiveDictionary(k => k.Key, v => v.Value.ToString());

            Request = new ScriptContextRequest(url:_uri, route:_routeDictionary, query:_queryDictionary, header:_headersDictionary, body:_parsedBody);
        }

        public void AttachResponse(IHeaderDictionary headers, JToken body)
        {
            Response = new ScriptContextResponse(headers.ToPermissiveDictionary(k => k.Key, v => v.Value.ToString()), body);
        }

        private static JObject ParseBody(HttpRequest httpRequest)
        {
            if (httpRequest.Body?.CanRead == false)
            {
                return new JObject();
            }

            if (string.IsNullOrEmpty(httpRequest.ContentType))
            {
                return new JObject();
            }
            
            if (httpRequest.HasFormContentType)
            {
                return ParseFormDataBody(httpRequest);
            }

            if (httpRequest.HasJsonContentType())
            {
                return ParseJsonBody(httpRequest);
            }

            return new JObject();
        }

        private static JObject ParseFormDataBody(HttpRequest httpRequest)
        {
            return JObject.FromObject(httpRequest.Form.ToDictionary(f => f.Key, f => f.Value.ToString()));
        }

        private static JObject ParseJsonBody(HttpRequest httpRequest)
        {
            httpRequest.EnableRewind();

            var reader = new StreamReader(httpRequest.Body);

            var json = reader.ReadToEnd();

            if (httpRequest.Body.CanSeek)
            {
                httpRequest.Body.Seek(0, SeekOrigin.Begin);
            }

            return !string.IsNullOrWhiteSpace(json) ? JObject.Parse(json) : new JObject();
        }
    }
}