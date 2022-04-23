using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Mockaco
{
    public class ResponseDelayMiddleware
    {
        private readonly RequestDelegate _next;

        public ResponseDelayMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, IMockacoContext mockacoContext, ILogger<ResponseDelayMiddleware> logger)
        {
            var transformedTemplate = mockacoContext.TransformedTemplate;
            if (transformedTemplate == default)
            {
                return;
            }

            int responseDelay = transformedTemplate.Response?.Delay.GetValueOrDefault() ?? 0;
            if (responseDelay > 0)
            {
                logger.LogDebug("Response delay: {responseDelay} milliseconds", responseDelay);

                await Task.Delay(responseDelay);
            }

            await _next(httpContext);
        }
    }
}
