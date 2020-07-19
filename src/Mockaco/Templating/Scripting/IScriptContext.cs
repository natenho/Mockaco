using Bogus;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Mockaco
{
    public interface IScriptContext
    {
        Faker Faker { get; }

        ScriptContextRequest Request { get; set; }

        ScriptContextResponse Response { get; set; }

        Task AttachRequest(HttpRequest httpRequest);

        Task AttachRouteParameters(HttpRequest httpRequest, Mock route);

        Task AttachResponse(HttpResponse httpResponse, ResponseTemplate responseTemplate);
    }
}