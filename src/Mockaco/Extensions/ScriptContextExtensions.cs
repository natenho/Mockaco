using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mockaco
{
    public static class ScriptContextExtensions
    {
        public static async Task AttachRequest(this IScriptContext scriptContext, HttpRequest httpRequest, IFakerFactory fakerFactory, IRequestBodyFactory requestBodyFactory)
        {
            var uri = httpRequest.GetUri();
            var queryDictionary = httpRequest.Query.ToStringDictionary(k => k.Key, v => v.Value.ToString());
            var headersDictionary = httpRequest.Headers.ToStringDictionary(k => k.Key, v => v.Value.ToString());
            var bodyAsJson = await requestBodyFactory.ReadBodyAsJson(httpRequest);

            //scriptContext.Faker = fakerFactory?.GetFaker(httpRequest.GetAcceptLanguageValues());
            scriptContext.Request = new ScriptContextRequest(url: uri, route: scriptContext.Request.Route, query: queryDictionary, header: headersDictionary, body: bodyAsJson);
        }

        public static Task AttachRouteParameters(this IScriptContext scriptContext, HttpRequest httpRequest, Mock mock)
        {
            scriptContext.Request
                .AttachRouteParams(httpRequest.GetRouteData(mock).ToStringDictionary(k => k.Key, v => v.Value.ToString()));

            return Task.CompletedTask;
        }

        public static Task AttachResponse(this IScriptContext scriptContext, HttpResponse httpResponse, ResponseTemplate responseTemplate)
        {
            scriptContext.Response = new ScriptContextResponse(httpResponse.Headers.ToStringDictionary(k => k.Key, v => v.Value.ToString()), responseTemplate?.Body);

            return Task.CompletedTask;
        }
    }
}
