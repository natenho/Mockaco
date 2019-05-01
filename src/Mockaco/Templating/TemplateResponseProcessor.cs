using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;

namespace Mockaco
{
    public class TemplateResponseProcessor : ITemplateResponseProcessor
    {
        private readonly ILogger<TemplateResponseProcessor> _logger;

        public TemplateResponseProcessor(
            IMockacoContext mockacoContext,
            ILogger<TemplateResponseProcessor> logger)
        {
            _logger = logger;
        }

        public async Task PrepareResponse(HttpResponse httpResponse, IScriptContext scriptContext, Template template)
        {
            httpResponse.StatusCode = template.Response.Status == 0
                ? (int)HttpStatusCode.OK
                : (int)template.Response.Status;

            WriteResponseHeaders(httpResponse, template);

            await WriteResponseBody(httpResponse, template);

            scriptContext.AttachResponse(httpResponse.Headers, template.Response.Body);
        }

        private void WriteResponseHeaders(HttpResponse response, Template template)
        {
            if (template.Response.Headers != null)
            {
                foreach (var header in template.Response.Headers)
                {
                    response.Headers.Add(header.Key, header.Value);
                }
            }

            if (string.IsNullOrEmpty(response.ContentType))
            {
                response.ContentType = "application/json";
            }
        }

        private async Task WriteResponseBody(HttpResponse response, Template template)
        {
            var responseBody = template.Response.Body?.ToString();

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