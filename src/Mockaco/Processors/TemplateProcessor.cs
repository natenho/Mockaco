using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mockaco.Processors;
using Mockaco;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
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
            var stopwatch = Stopwatch.StartNew();

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

                    mockacoContext.ResponseDelay = int.Parse(ProcessResponsePart(template.Response.Delay, scriptContext));

                    await PrepareResponse(httpContext.Response, scriptContext, template);

                    return;
                }
            }

            throw new InvalidOperationException("No templates matching the request");
        }

        // TODO Refactor SRP violation
        private async Task PrepareResponse(HttpResponse response, ScriptContext scriptContext, Template template)
        {
            response.StatusCode = template.Response.Status == 0 ? (int) HttpStatusCode.OK : (int) template.Response.Status;

            await PrepareHeaders(response, scriptContext, template);

            await PrepareBody(response, scriptContext, template);
        }

        private async Task PrepareHeaders(HttpResponse response, ScriptContext scriptContext, Template template)
        {
            if (template.Response.Headers != null)
            {
                foreach (var header in template.Response.Headers)
                {
                    var key = await _templateTransformer.Transform(header.Key, scriptContext);
                    var value = await _templateTransformer.Transform(header.Value, scriptContext);

                    if (response.Headers.ContainsKey(key))
                    {
                        response.Headers.Remove(key);
                    }

                    response.Headers.Add(key, value);
                }
            }

            if (string.IsNullOrEmpty(response.ContentType))
            {
                response.ContentType = "application/json";
            }
        }

        private async Task PrepareBody(HttpResponse response, ScriptContext scriptContext, Template template)
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
                if (!(conditionMatches is bool) || !(bool) conditionMatches)
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