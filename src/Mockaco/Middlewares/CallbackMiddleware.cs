using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
            ITemplateTransformer templateTransformer,
            IOptionsSnapshot<MockacoOptions> options)
        {
            if (mockacoContext.TransformedTemplate?.Callbacks?.Any() != true)
            {
                return Task.CompletedTask;
            }

            httpContext.Response.OnCompleted(
                () =>
                {
                    //TODO Refactor to avoid method with too many parameters (maybe a CallbackRunnerFactory?)
                    var fireAndForgetTask = PerformCallbacks(httpContext, mockacoContext, scriptContext, templateTransformer, options.Value);
                    return Task.CompletedTask;
                });

            return Task.CompletedTask;
        }
        
        private async Task PerformCallbacks(
            HttpContext httpContext,
            IMockacoContext mockacoContext,
            IScriptContext scriptContext,
            ITemplateTransformer templateTransformer,
            MockacoOptions options)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                                
                var template = await templateTransformer.Transform(mockacoContext.Mock.RawTemplate, scriptContext);

                var callbackTasks = new List<Task>();

                foreach (var callbackTemplate in template.Callbacks)
                {
                    callbackTasks.Add(PerformCallback(httpContext, callbackTemplate, options, stopwatch.ElapsedMilliseconds));
                }

                await Task.WhenAll(callbackTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing callback(s)");
            }
        }

        private async Task PerformCallback(HttpContext httpContext, CallbackTemplate callbackTemplate, MockacoOptions options, long elapsedMilliseconds)
        {
            try
            {
                var request = PrepareHttpRequest(callbackTemplate, options);

                var httpClient = PrepareHttpClient(httpContext, callbackTemplate);

                await DelayRequest(callbackTemplate, elapsedMilliseconds);

                var stopwatch = Stopwatch.StartNew();

                _logger.LogDebug("Callback started");

                await PerformRequest(request, httpClient);

                _logger.LogDebug("Callback finished in {0} ms", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Callback error");
            }
        }

        private static HttpRequestMessage PrepareHttpRequest(CallbackTemplate callbackTemplate, MockacoOptions options)
        {
            var request = new HttpRequestMessage(new HttpMethod(callbackTemplate.Method), callbackTemplate.Url);

            var formatting = callbackTemplate.Indented.GetValueOrDefault(true) ? Formatting.Indented : default;

            if (callbackTemplate.Body != null)
            {
                request.Content = callbackTemplate.Headers?.ContainsKey(HttpHeaders.ContentType) == true
                    ? new StringContent(callbackTemplate.Body.ToString(), Encoding.UTF8, callbackTemplate.Headers[HttpHeaders.ContentType])
                    : new StringContent(callbackTemplate.Body.ToString(formatting));
            }

            PrepareHeaders(callbackTemplate, request, options);

            return request;
        }

        private static void PrepareHeaders(CallbackTemplate callBackTemplate, HttpRequestMessage httpRequest, MockacoOptions options)
        {
            if (callBackTemplate.Headers != null)
            {
                foreach (var header in callBackTemplate.Headers.Where(h => h.Key != HttpHeaders.ContentType))
                {
                    if (httpRequest.Headers.Contains(header.Key))
                    {
                        httpRequest.Headers.Remove(header.Key);
                    }

                    httpRequest.Headers.Add(header.Key, header.Value);
                }
            }

            if (!httpRequest.Headers.Accept.Any())
            {
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(options.DefaultHttpContentType));
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
                _logger.LogDebug("Callback delay: {0} milliseconds", remainingDelay.TotalMilliseconds);
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
            catch (OperationCanceledException)
            {
                _logger.LogError("Callback request timeout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Callback error");
            }
        }
    }
}