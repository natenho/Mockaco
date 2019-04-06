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

        public async Task Invoke(HttpContext httpContext, ITemplateProcessor templateProcessor)
        {
            await templateProcessor.ProcessResponse(httpContext);

            await _next(httpContext);
        }
    }
}
