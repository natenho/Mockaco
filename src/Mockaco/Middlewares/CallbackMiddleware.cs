using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mockaco.Processors;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Mockaco
{
    public class CallbackMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CallbackMiddleware> _logger;

        public CallbackMiddleware(RequestDelegate next, ILogger<CallbackMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public Task Invoke(
            HttpContext httpContext,
            IMockacoContext mockacoContext,
            IScriptContext scriptContext,
            ITemplateTransformer templateTransformer)
        {
            if (mockacoContext.TransformedTemplate?.Callback == null)
            {
                return Task.CompletedTask;
            }

            httpContext.Response.OnCompleted(() =>
            {
                var fireAndForgetTask = PerformCallback(httpContext, mockacoContext, scriptContext, templateTransformer);
                return Task.CompletedTask;
            });

            return Task.CompletedTask;
        }

        private async Task PerformCallback(
            HttpContext httpContext,
            IMockacoContext mockacoContext,
            IScriptContext scriptContext,
            ITemplateTransformer templateTransformer)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                var callbackTemplate = await PrepareCallbackTemplate(mockacoContext, scriptContext, templateTransformer);

                var request = PrepareHttpRequest(callbackTemplate);

                var httpClient = PrepareHttpClient(httpContext, callbackTemplate);

                await DelayRequest(callbackTemplate, stopwatch.ElapsedMilliseconds);

                stopwatch.Restart();

                _logger.LogDebug("Callback starting");

                await PerformRequest(request, httpClient);

                _logger.LogDebug("Callback finished in {0} ms", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Callback error");
            }
        }

        private static async Task<CallbackTemplate> PrepareCallbackTemplate(IMockacoContext mockacoContext, IScriptContext scriptContext, ITemplateTransformer templateTransformer)
        {
            var template = await templateTransformer.Transform(mockacoContext.Route.RawTemplate, scriptContext);

            return template.Callback;
        }

        private static HttpRequestMessage PrepareHttpRequest(CallbackTemplate callbackTemplate)
        {
            var request = new HttpRequestMessage(
                new HttpMethod(callbackTemplate.Method),
                callbackTemplate.Url);

            if (callbackTemplate.Body != null)
            {
                request.Content = new StringContent(callbackTemplate.Body.ToString());
            }

            PrepareHeaders(callbackTemplate, request);

            return request;
        }

        private static void PrepareHeaders(CallbackTemplate callBackTemplate, HttpRequestMessage httpRequest)
        {
            if (callBackTemplate.Headers != null)
            {
                foreach (var header in callBackTemplate.Headers)
                {
                    if (httpRequest.Content.Headers.Contains(header.Key))
                    {
                        httpRequest.Content.Headers.Remove(header.Key);
                    }

                    httpRequest.Content.Headers.Add(header.Key, header.Value);
                }
            }

            if (!httpRequest.Headers.Accept.Any())
            {
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
        }

        private static HttpClient PrepareHttpClient(HttpContext httpContext, CallbackTemplate callbackTemplate)
        {
            var factory = httpContext.RequestServices.GetService<IHttpClientFactory>();
            var httpClient = factory.CreateClient();

            httpClient.Timeout = TimeSpan.FromMilliseconds(callbackTemplate.Timeout.GetValueOrDefault());

            return httpClient;
        }

        private async Task DelayRequest(CallbackTemplate callbackTemplate, long elapsedMilliseconds)
        {
            var remainingDelay = TimeSpan.FromMilliseconds(callbackTemplate.Delay.GetValueOrDefault() - elapsedMilliseconds);
            if (elapsedMilliseconds < remainingDelay.TotalMilliseconds)
            {
                _logger.LogDebug("Waiting {0} ms to perform callback on time", remainingDelay.TotalMilliseconds);
                await Task.Delay(remainingDelay);
            }
        }

        private async Task PerformRequest(HttpRequestMessage request, HttpClient httpClient)
        {
            try
            {
                var response = await httpClient.SendAsync(request);

                _logger.LogDebug("Callback response\n\n{0}\n", response);
                _logger.LogDebug("Callback response content\n\n{0}\n", await response.Content.ReadAsStringAsync());
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "Callback request timeout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Callback error");
            }
        }
    }
}
