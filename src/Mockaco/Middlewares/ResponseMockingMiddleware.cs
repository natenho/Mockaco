using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Mockaco
{
    public class ResponseMockingMiddleware
    {
        private readonly RequestDelegate _next;

        public ResponseMockingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, IMockacoContext mockacoContext, IScriptContext scriptContext,  ITemplateResponseProcessor templateProcessor)
        {            
            await templateProcessor.PrepareResponse(httpContext.Response, scriptContext, mockacoContext.TransformedTemplate);

            await _next(httpContext);
        }
    }
}
