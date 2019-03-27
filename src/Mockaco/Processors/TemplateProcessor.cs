using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mockaco;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mockore
{
    public class TemplateProcessor : ITemplateProcessor
    {
        private readonly IScriptRunnerFactory _scriptRunnerFactory;
        private readonly ILogger<TemplateProcessor> _logger;
        private readonly IEnumerable<Template> _templates;

        public TemplateProcessor(
            ITemplateRepository templateRepository,
            IScriptRunnerFactory scriptRunnerFactory,
            ILogger<TemplateProcessor> logger)
        {
            _scriptRunnerFactory = scriptRunnerFactory;
            _logger = logger;
            _templates = templateRepository.GetAll();
        }

        public async Task ProcessResponse(HttpContext httpContext)
        {
            var stopwatch = Stopwatch.StartNew();

            var scriptContext = new ScriptContext(httpContext);

            foreach (var template in _templates.OrderByDescending(t => t.Request.Condition?.Length)) // TODO Implement priority 
            {
                if (await RequestMatchesTemplate(httpContext, template.Request, scriptContext).ConfigureAwait(false))
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
            response.ContentType = "application/json"; // TODO Can we support other content types ? XML ?
            response.StatusCode = template.Response.Status == 0 ? (int)HttpStatusCode.OK : (int)template.Response.Status;

            if (template.Response.Headers != null)
            {
                foreach (var header in template.Response.Headers)
                {
                    string key = ProcessResponsePart(header.Key, scriptContext);
                    string value = ProcessResponsePart(header.Value, scriptContext);

                    response.Headers.Add(key, value);
                }
            }

            var responseBody = ProcessResponsePart(template.Response.Body?.ToString(), scriptContext);
            if (responseBody != null)
            {
                await response.WriteAsync(responseBody).ConfigureAwait(false);
            }
        }

        // TODO Refactor SRP violation
        private string ProcessResponsePart(string input, ScriptContext scriptContext)
        {
            const string codeRegex = @"(?=\""?)\$\{(?<code>.*?)\}(?=\""?)";

            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            return Regex.Replace(
                input,
                codeRegex,
                match =>
                {
                    var code = Regex.Unescape(
                        match.Groups["code"]
                            .ToString()); // TODO Fix hybrid value behavior i.e. "FOO ${Fake.Name.FullName()} BAR" is not working

                    var result = Run(code, scriptContext)
                        .GetAwaiter()
                        .GetResult(); // TODO Make async

                    // TODO Reflect results (make generated variables available to be added somewhere else)

                    return result.ToString();
                });
        }

        private string ProcessResponsePartAsJson(string input, ScriptContext scriptContext)
        {
            return JsonConvert.SerializeObject(ProcessResponsePart(input, scriptContext), Formatting.None, new JsonSerializerSettings());
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