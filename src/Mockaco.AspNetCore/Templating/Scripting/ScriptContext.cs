using Bogus;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mockaco
{
    public class ScriptContext : IScriptContext
    {
        private Uri _uri;
        private StringDictionary _queryDictionary = new StringDictionary();
        private StringDictionary _headersDictionary = new StringDictionary();
        private StringDictionary _routeDictionary = new StringDictionary();
        private JToken _bodyAsJson;
        private readonly IFakerFactory _fakerFactory;
        private readonly IRequestBodyFactory _requestBodyFactory;

        public IGlobalVariableStorage Global { get; }

        public Faker Faker { get; private set; }

        public ScriptContextRequest Request { get; set; }

        public ScriptContextResponse Response { get; set; }

        public ScriptContext(IFakerFactory fakerFactory, IRequestBodyFactory requestBodyFactory, IGlobalVariableStorage globalVarialeStorage)
        {
            _fakerFactory = fakerFactory;
            _requestBodyFactory = requestBodyFactory;
            Faker = fakerFactory?.GetDefaultFaker();
            Global = globalVarialeStorage;

            Request = new ScriptContextRequest(
                default,
                new StringDictionary(),
                new StringDictionary(),
                new StringDictionary(),
                new JObject());

            Response = new ScriptContextResponse(new StringDictionary(), new JObject());
        }

        public async Task AttachRequest(HttpRequest httpRequest)
        {
            _uri = httpRequest.GetUri();
            _queryDictionary = httpRequest.Query.ToStringDictionary(k => k.Key, v => v.Value.ToString());
            _headersDictionary = httpRequest.Headers.ToStringDictionary(k => k.Key, v => v.Value.ToString());
            _bodyAsJson = await _requestBodyFactory.ReadBodyAsJson(httpRequest);

            Faker = _fakerFactory?.GetFaker(httpRequest.GetAcceptLanguageValues());

            Request = new ScriptContextRequest(url: _uri, route: _routeDictionary, query: _queryDictionary, header: _headersDictionary, body: _bodyAsJson);
        }

        public Task AttachRouteParameters(HttpRequest httpRequest, Mock mock)
        {
            _routeDictionary = httpRequest.GetRouteData(mock)
                .ToStringDictionary(k => k.Key, v => v.Value.ToString());

            Request = new ScriptContextRequest(url: _uri, route: _routeDictionary, query: _queryDictionary, header: _headersDictionary, body: _bodyAsJson);

            return Task.CompletedTask;
        }

        public Task AttachResponse(HttpResponse httpResponse, ResponseTemplate responseTemplate)
        {
            Response = new ScriptContextResponse(httpResponse.Headers.ToStringDictionary(k => k.Key, v => v.Value.ToString()), responseTemplate?.Body);

            return Task.CompletedTask;
        }
    }
}