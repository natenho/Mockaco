using Bogus;
using Microsoft.AspNetCore.Http;
using Mockaco.Routing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mockaco
{
    public class ScriptContext : IScriptContext
    {
        private Uri _uri;
        private StringDictionary _queryDictionary = new StringDictionary();
        private StringDictionary _headersDictionary = new StringDictionary();
        private StringDictionary _routeDictionary = new StringDictionary();
        private JObject _parsedBody;

        public Faker Faker { get; }

        public ScriptContextRequest Request { get; set; }

        public ScriptContextResponse Response { get; set; }

        public ScriptContext()
        {
            Faker = new Faker("pt_BR"); // TODO Localize based on the request

            Request = new ScriptContextRequest(
                default,
                new StringDictionary(),
                new StringDictionary(),
                new StringDictionary(),
                new JObject());

            Response = new ScriptContextResponse(new StringDictionary(), new JObject());
        }

        public void AttachHttpContext(HttpContext httpContext)
        {
            _uri = httpContext.Request.GetUri();
            _queryDictionary = httpContext.Request.Query.ToStringDictionary(k => k.Key, v => v.Value.ToString());
            _headersDictionary = httpContext.Request.Headers.ToStringDictionary(k => k.Key, v => v.Value.ToString());
            _parsedBody = ParseBody(httpContext.Request);

            Request = new ScriptContextRequest(url:_uri, route:_routeDictionary, query:_queryDictionary, header:_headersDictionary, body:_parsedBody);
        }

        public void AttachRoute(HttpContext httpContext, Route route)
        {
            _routeDictionary = httpContext.Request.GetRouteData(route)
                .ToStringDictionary(k => k.Key, v => v.Value.ToString());

            Request = new ScriptContextRequest(url:_uri, route:_routeDictionary, query:_queryDictionary, header:_headersDictionary, body:_parsedBody);
        }

        public void AttachResponse(IHeaderDictionary headers, JToken body)
        {
            Response = new ScriptContextResponse(headers.ToStringDictionary(k => k.Key, v => v.Value.ToString()), body);
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
            var body = httpRequest.ReadBodyStream();
   
            return !string.IsNullOrWhiteSpace(body) ? JObject.Parse(body) : new JObject();
        }
    }
}