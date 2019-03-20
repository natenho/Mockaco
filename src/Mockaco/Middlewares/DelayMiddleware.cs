using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Mockaco
{
    public class DelayMiddleware
    {
        private readonly RequestDelegate _next;

        public DelayMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, MockacoContext mockacoContext, ILogger<DelayMiddleware> logger)
        {
            var stopwatch = Stopwatch.StartNew();

            await _next(httpContext);
                       
            var remainingDelay = mockacoContext.ResponseDelay - (int)stopwatch.ElapsedMilliseconds;
            if (remainingDelay > 0)
            {
                logger.LogDebug($"Delaying the response for at least {mockacoContext.ResponseDelay} milliseconds");

                await Task.Delay(remainingDelay);
            }
        }
    }
}
