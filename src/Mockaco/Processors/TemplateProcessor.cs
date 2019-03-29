using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mockaco.Processors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Mockaco
{
    public class TemplateProcessor : ITemplateProcessor
    {
        private readonly IScriptRunnerFactory _scriptRunnerFactory;
        private readonly ITemplateTransformer _templateTransformer;
        private readonly ILogger<TemplateProcessor> _logger;
        private readonly IEnumerable<TemplateFile> _templates;

        public TemplateProcessor(
            ITemplateRepository templateRepository,
            IScriptRunnerFactory scriptRunnerFactory,
            ITemplateTransformer templateTransformer,
            ILogger<TemplateProcessor> logger)
        {
            _scriptRunnerFactory = scriptRunnerFactory;
            _templateTransformer = templateTransformer;
            _logger = logger;
            _scriptRunnerFactory = scriptRunnerFactory;
            _logger = logger;
            _templates = templateRepository.GetAll();
        }

        public async Task ProcessResponse(HttpContext httpContext)
        {           
            var scriptContext = new ScriptContext(httpContext);

            var transformedTemplates = new List<Template>();

            foreach (var templateFile in _templates)
            {
                var transformedTemplate = await _templateTransformer.Transform(templateFile.Content, scriptContext);
                var template = JsonConvert.DeserializeObject<Template>(transformedTemplate);
                transformedTemplates.Add(template);
            }

            foreach (var template in transformedTemplates.OrderByDescending(t => t.Request.Condition?.Length)) // TODO Implement priority 
            {
                if (await RequestMatchesTemplate(httpContext, template.Request, scriptContext)
                    .ConfigureAwait(false))
                {
                    var mockacoContext = httpContext.RequestServices.GetRequiredService<MockacoContext>();

                    string delayString = await _templateTransformer.Transform(template.Response.Delay, scriptContext);

                    if (int.TryParse(delayString, out var delay))
                    {
                        mockacoContext.ResponseDelay = delay;
                    }

                    await PrepareResponse(httpContext.Response, scriptContext, template);

                    if (template.Callback != null)
                    {
                        httpContext.Response.OnCompleted(PerformCallback(httpContext, scriptContext, template));
                    }

                    return;
                }
            }

            throw new InvalidOperationException("No templates matching the request");
        }

        private Func<Task> PerformCallback(HttpContext httpContext, ScriptContext scriptContext, Template template)
        {
            return async () =>
            {
                var stopwatch = Stopwatch.StartNew();

                var factory = httpContext.RequestServices.GetService<IHttpClientFactory>();

                var httpClient = factory.CreateClient();

                if (int.TryParse(template.Callback.Timeout, out var timeout))
                {
                    httpClient.Timeout = TimeSpan.FromMilliseconds(timeout);
                }

                var request = new HttpRequestMessage(
                    new HttpMethod(template.Callback.Method),
                    template.Callback.Url);

                await PrepareCallbackHeaders(scriptContext, template, request);

                if(template.Callback.Body != null)
                {
                    request.Content = new StringContent(template.Callback.Body.ToString());
                }

                if (int.TryParse(template.Callback.Delay, out var delay))
                {
                    var remainingDelay = TimeSpan.FromMilliseconds(delay - stopwatch.ElapsedMilliseconds);
                    if (stopwatch.ElapsedMilliseconds < remainingDelay.TotalMilliseconds)
                    {
                        _logger.LogDebug("Waiting {0} ms to perform callback on time", remainingDelay.TotalMilliseconds);
                        await Task.Delay(remainingDelay);
                    }
                }
                
                stopwatch.Restart();

                _logger.LogDebug("Callback starting");

                try
                {
                    var response = await httpClient.SendAsync(request);

                    _logger.LogDebug("Callback response {0}", response);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Callback error");
                }

                _logger.LogDebug("Callback finished in {0} ms", stopwatch.ElapsedMilliseconds);

                return;
            };
        }

        private async Task PrepareCallbackHeaders(ScriptContext scriptContext, Template template, HttpRequestMessage httpRequest)
        {
            var headers = await TransformHeaders(scriptContext, template.Callback.Headers);

            foreach (var header in headers)
            {
                httpRequest.Headers.Add(header.Key, header.Value);
            }

            if (!httpRequest.Headers.Accept.Any())
            {
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
        }

        // TODO Refactor SRP violation
        private async Task PrepareResponse(HttpResponse response, ScriptContext scriptContext, Template template)
        {
            response.StatusCode = template.Response.Status == 0 ? (int)HttpStatusCode.OK : (int)template.Response.Status;

            await PrepareResponseHeaders(response, scriptContext, template);

            await PrepareResponseBody(response, scriptContext, template);
        }

        private async Task PrepareResponseHeaders(HttpResponse response, ScriptContext scriptContext, Template template)
        {
            var headers = await TransformHeaders(scriptContext, template.Response.Headers);

            foreach (var header in headers)
            {
                response.Headers.Add(header.Key, header.Value);
            }

            if (string.IsNullOrEmpty(response.ContentType))
            {
                response.ContentType = "application/json";
            }
        }

        private async Task<IDictionary<string, string>> TransformHeaders(
            ScriptContext scriptContext,
            IDictionary<string, string> inputDictionary)
        {
            var outputDictionary = new PermissiveDictionary<string, string>();

            if (inputDictionary == null)
            {
                return outputDictionary;
            }

            foreach (var header in inputDictionary)
            {
                var key = await _templateTransformer.Transform(header.Key, scriptContext);
                var value = await _templateTransformer.Transform(header.Value, scriptContext);

                outputDictionary.Add(key, value);
            }

            return outputDictionary;
        }

        private async Task PrepareResponseBody(HttpResponse response, ScriptContext scriptContext, Template template)
        {
            var responseBody = await _templateTransformer.Transform(template.Response.Body?.ToString(CultureInfo.InvariantCulture), scriptContext);

            if (response.ContentType != "application/json")
            {
                responseBody = JsonConvert.DeserializeObject<string>(responseBody);
            }

            if (responseBody != null)
            {
                await response.WriteAsync(responseBody)
                    .ConfigureAwait(false);
            }
        }

        // TODO Refactor SRP violation
        private async Task<bool> RequestMatchesTemplate(HttpContext httpContext, RequestTemplate requestTemplate, ScriptContext scriptContext)
        {
            if (requestTemplate.Method != null)
            {
                var methodMatches = httpContext.Request.Method.Equals(requestTemplate.Method.ToString(), StringComparison.InvariantCultureIgnoreCase);
                if (!methodMatches)
                {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(requestTemplate.Route))
            {
                var routeMatcher = new RouteMatcher();
                var routeMatches = routeMatcher.IsMatch(requestTemplate.Route, httpContext.Request.Path);
                if (!routeMatches)
                {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(requestTemplate.Condition))
            {
                var conditionMatches = await Run(requestTemplate.Condition, scriptContext);
                if (!(conditionMatches is bool) || !(bool)conditionMatches)
                {
                    return false;
                }
            }

            return true;
        }

        // TODO Remove repeated code
        private async Task<object> Run(string code, ScriptContext scriptContext)
        {
            object result = null;

            try
            {
                result = await _scriptRunnerFactory.Invoke<ScriptContext, object>(scriptContext, code);

                _logger.LogDebug($"Processed script {code} with result {result}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Processed script {code} with result {ex}");
            }

            return result;
        }
    }
}