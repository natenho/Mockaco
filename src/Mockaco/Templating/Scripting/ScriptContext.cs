using Bogus;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Mockaco
{
    public class ScriptContext : IScriptContext
    {
        private Uri _uri;
        private StringDictionary _queryDictionary = new StringDictionary();
        private StringDictionary _headersDictionary = new StringDictionary();
        private StringDictionary _routeDictionary = new StringDictionary();
        private JObject _bodyAsJson;
        private readonly IFakerFactory _fakerFactory;
        private readonly IRequestBodyFactory _requestBodyFactory;

        public Faker Faker { get; private set; }

        public ScriptContextRequest Request { get; set; }

        public ScriptContextResponse Response { get; set; }

        public ScriptContext(IFakerFactory fakerFactory, IRequestBodyFactory requestBodyFactory)
        {
            _fakerFactory = fakerFactory;
            _requestBodyFactory = requestBodyFactory;
            Faker = fakerFactory?.GetDefaultFaker();

            Request = new ScriptContextRequest(
                default,
                new StringDictionary(),
                new StringDictionary(),
                new StringDictionary(),
                new JObject());

            Response = new ScriptContextResponse(new StringDictionary(), new JObject());
        }

        public void AttachRequest(HttpRequest httpRequest)
        {
            _uri = httpRequest.GetUri();
            _queryDictionary = httpRequest.Query.ToStringDictionary(k => k.Key, v => v.Value.ToString());
            _headersDictionary = httpRequest.Headers.ToStringDictionary(k => k.Key, v => v.Value.ToString());
            _bodyAsJson = _requestBodyFactory.ReadBodyAsJson(httpRequest);

            Faker = _fakerFactory?.GetFaker(httpRequest.GetAcceptLanguageValues());

            Request = new ScriptContextRequest(url: _uri, route: _routeDictionary, query: _queryDictionary, header: _headersDictionary, body: _bodyAsJson);
        }

        public void AttachRouteParameters(HttpRequest httpRequest, Mock route)
        {
            _routeDictionary = httpRequest.GetRouteData(route)
                .ToStringDictionary(k => k.Key, v => v.Value.ToString());

            Request = new ScriptContextRequest(url: _uri, route: _routeDictionary, query: _queryDictionary, header: _headersDictionary, body: _bodyAsJson);
        }

        public void AttachResponse(HttpResponse httpResponse, ResponseTemplate responseTemplate)
        {
            Response = new ScriptContextResponse(httpResponse.Headers.ToStringDictionary(k => k.Key, v => v.Value.ToString()), responseTemplate.Body);
        }
    }
}