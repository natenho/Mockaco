using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
                if (!await RequestMatchesTemplate(httpContext, template.Request, scriptContext).ConfigureAwait(false))
                {
                    continue;
                }

                httpContext.Response.StatusCode = (int)template.Response.Status == 0 ? (int)HttpStatusCode.OK : (int)template.Response.Status;
                httpContext.Response.ContentType = "application/json"; // TODO Can we support other content types ? XML ?

                var responseBody = ProcessResponseBody(template.Response, scriptContext);
                if (responseBody != null)
                {
                    await httpContext.Response.WriteAsync(responseBody).ConfigureAwait(false);
                }

                // TODO Refactor SRP (Middleware?)
                while (stopwatch.ElapsedMilliseconds < template.Response.Delay)
                {
                    await Task.Delay(10).ConfigureAwait(false);
                }

                return;
            }

            throw new InvalidOperationException("No templates matching the request");
        }

        private string ProcessResponseBody(ResponseTemplate responseTemplate, ScriptContext scriptContext)
        {
            const string codeRegex = @"(\""?)\$\{(?<code>.*)\}(\""?)";

            if (responseTemplate.Body == null)
            {
                return null;
            }

            var body = responseTemplate.Body.ToString();

            return Regex.Replace(
                body,
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

                    return JsonConvert.SerializeObject(result, Formatting.None, new JsonSerializerSettings());
                });
        }

        // TODO Refactor SRP
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