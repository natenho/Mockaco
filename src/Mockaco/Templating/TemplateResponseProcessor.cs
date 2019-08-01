using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net;
using System.Threading.Tasks;

namespace Mockaco
{
    public class TemplateResponseProcessor : ITemplateResponseProcessor
    {
        public async Task PrepareResponse(HttpResponse httpResponse, IScriptContext scriptContext, Template template)
        {
            httpResponse.StatusCode = template.Response.Status == default
                ? (int)HttpStatusCode.OK
                : (int)template.Response.Status;

            WriteResponseHeaders(httpResponse, template.Response);

            await WriteResponseBody(httpResponse, template.Response);

            scriptContext.AttachResponse(httpResponse.Headers, template.Response.Body);
        }

        private void WriteResponseHeaders(HttpResponse response, ResponseTemplate responseTemplate)
        {
            if (responseTemplate.Headers != null)
            {
                foreach (var header in responseTemplate.Headers)
                {
                    response.Headers.Add(header.Key, header.Value);
                }
            }

            if (string.IsNullOrEmpty(response.ContentType))
            {
                response.ContentType = "application/json";
            }
        }

        private async Task WriteResponseBody(HttpResponse response, ResponseTemplate responseTemplate)
        {
            var formatting = responseTemplate.Indented.GetValueOrDefault() ? Formatting.Indented : default;

            var responseBody = responseTemplate.Body?.ToString(formatting);

            if (response.ContentType != "application/json")
            {
                responseBody = JsonConvert.DeserializeObject<string>(responseBody);
            }

            if (responseBody != null)
            {
                await response.WriteAsync(responseBody);
            }
        }
    }
}