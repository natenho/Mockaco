using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Mockaco.Middlewares
{
    public class TemplateTransformationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TemplateTransformationMiddleware> _logger;

        public TemplateTransformationMiddleware(RequestDelegate next, ILogger<TemplateTransformationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext, MockacoContext mockacoContext, TemplateTransformationService templateTransformationService)
        {            
            await templateTransformationService.Process(httpContext, mockacoContext);

            await _next(httpContext);
        }
    }
}
