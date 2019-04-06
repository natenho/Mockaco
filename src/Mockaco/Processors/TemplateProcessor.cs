using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;

namespace Mockaco
{
    public class TemplateProcessor : ITemplateProcessor
    {
        private readonly MockacoContext _mockacoContext;
        private readonly ILogger<TemplateProcessor> _logger;

        public TemplateProcessor(
            MockacoContext mockacoContext,
            ILogger<TemplateProcessor> logger)
        {
            _mockacoContext = mockacoContext;
            _logger = logger;
        }

        public async Task ProcessResponse(HttpContext httpContext)
        {
            await PrepareResponse(httpContext.Response, _mockacoContext.ScriptContext, _mockacoContext.Template);
        }

        private async Task PrepareResponse(HttpResponse response, ScriptContext scriptContext, Template template)
        {
            response.StatusCode = template.Response.Status == 0 ? (int)HttpStatusCode.OK : (int)template.Response.Status;

            await WriteResponseHeaders(response, scriptContext, template);

            await WriteResponseBody(response, scriptContext, template);
        }

        private async Task WriteResponseHeaders(HttpResponse response, ScriptContext scriptContext, Template template)
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
                var key = header.Key;
                var value = header.Value;

                outputDictionary.Add(key, value);
            }

            return outputDictionary;
        }

        private async Task WriteResponseBody(HttpResponse response, ScriptContext scriptContext, Template template)
        {
            var responseBody = template.Response.Body?.ToString(CultureInfo.InvariantCulture);

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
    }
}