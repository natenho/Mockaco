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

        void AttachHttpContext(HttpContext httpContext);

        void AttachRoute(HttpContext httpContext, Route route);

        void AttachResponse(IHeaderDictionary headers, JToken body);
    }
}