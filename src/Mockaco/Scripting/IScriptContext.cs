using Bogus;
using Microsoft.AspNetCore.Http;
using Mockaco.Routing;
using Newtonsoft.Json.Linq;

namespace Mockaco
{
    public interface IScriptContext
    {
        Faker Faker { get; }

        ScriptContextRequest Request { get; set; }

        ScriptContextResponse Response { get; set; }

        void AttachRequest(HttpRequest httpRequest);

        void AttachRoute(HttpRequest httpRequest, Route route);

        void AttachResponse(IHeaderDictionary headers, JToken body);
    }
}