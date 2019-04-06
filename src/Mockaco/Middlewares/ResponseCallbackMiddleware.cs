using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Mockaco
{
    public class ResponseCallbackMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ResponseCallbackMiddleware> _logger;

        public ResponseCallbackMiddleware(RequestDelegate next, ILogger<ResponseCallbackMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public Task Invoke(HttpContext httpContext, MockacoContext mockacoContext, ITemplateProcessor templateProcessor)
        {
            if (mockacoContext.Template?.Callback == null)
            {
                return Task.CompletedTask;
            }

            httpContext.Response.OnCompleted(() =>
            {
                var fireAndForgetTask = PerformCallback(httpContext, mockacoContext);
                return Task.CompletedTask;
            });

            return Task.CompletedTask;
        }

        private async Task PerformCallback(HttpContext httpContext, MockacoContext mockacoContext)
        {
            var stopwatch = Stopwatch.StartNew();

            var template = mockacoContext.Template;
            var scriptContext = mockacoContext.ScriptContext;

            var factory = httpContext.RequestServices.GetService<IHttpClientFactory>();

            var httpClient = factory.CreateClient();

            httpClient.Timeout = TimeSpan.FromMilliseconds(template.Callback.Timeout.GetValueOrDefault());

            var request = new HttpRequestMessage(
                new HttpMethod(template.Callback.Method),
                template.Callback.Url);

            PrepareCallbackHeaders(scriptContext, template, request);

            if (template.Callback.Body != null)
            {
                request.Content = new StringContent(template.Callback.Body.ToString());
            }

            var remainingDelay = TimeSpan.FromMilliseconds(template.Callback.Delay.GetValueOrDefault() - stopwatch.ElapsedMilliseconds);
            if (stopwatch.ElapsedMilliseconds < remainingDelay.TotalMilliseconds)
            {
                _logger.LogDebug("Waiting {0} ms to perform callback on time", remainingDelay.TotalMilliseconds);
                await Task.Delay(remainingDelay);
            }

            stopwatch.Restart();

            _logger.LogDebug("Callback starting");

            try
            {
                var response = await httpClient.SendAsync(request);

                _logger.LogDebug("Callback response {0}", response);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "Callback request timeout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Callback error");
            }

            _logger.LogDebug("Callback finished in {0} ms", stopwatch.ElapsedMilliseconds);
        }

        private void PrepareCallbackHeaders(ScriptContext scriptContext, Template template, HttpRequestMessage httpRequest)
        {
            foreach (var header in template.Callback.Headers)
            {
                httpRequest.Headers.Add(header.Key, header.Value);
            }

            if (!httpRequest.Headers.Accept.Any())
            {
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
        }
    }
}
