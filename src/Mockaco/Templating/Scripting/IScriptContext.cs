using Bogus;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace Mockaco
{
    public interface IScriptContext
    {
        Faker Faker { get; }

        ScriptContextRequest Request { get; set; }

        ScriptContextResponse Response { get; set; }

        void AttachRequest(HttpRequest httpRequest);

        void AttachRouteParameters(HttpRequest httpRequest, Mock route);

        void AttachResponse(HttpResponse httpResponse, ResponseTemplate responseTemplate);
    }
}