using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net;
using System.Threading.Tasks;

namespace Mockaco
{
    public class TemplateResponseProcessor : ITemplateResponseProcessor
    {
        private const string JsonContentType = "application/json";
        private const HttpStatusCode DefaultHttpStatusCode = HttpStatusCode.OK;

        public async Task PrepareResponse(HttpResponse httpResponse, IScriptContext scriptContext, Template template)
        {
            httpResponse.StatusCode = template.Response.Status == default
                ? (int)DefaultHttpStatusCode
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
                response.ContentType = JsonContentType;
            }
        }

        private async Task WriteResponseBody(HttpResponse response, ResponseTemplate responseTemplate)
        {
            if(responseTemplate.Body == null)
            {
                return;
            }

            string responseBody;

            //TODO Move to a factory
            if (response.ContentType == JsonContentType)
            {
                var formatting = responseTemplate.Indented.GetValueOrDefault() ? Formatting.Indented : default;

                responseBody = responseTemplate.Body?.ToString(formatting);
            }
            else
            {
                //Deserializes the JSON string to unescape the body into its raw value
                responseBody = JsonConvert.DeserializeObject<string>(responseTemplate.Body?.ToString());
            }

            await response.WriteAsync(responseBody);
        }
    }
}