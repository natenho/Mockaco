using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;

namespace Mockaco
{
    public class TemplateResponseProcessor : ITemplateResponseProcessor
    {        
        private const HttpStatusCode DefaultHttpStatusCode = HttpStatusCode.OK;

        private readonly IResponseBodyStrategyFactory _responseBodyFactory;

        public TemplateResponseProcessor(IResponseBodyStrategyFactory responseBodyFactory)
        {
            _responseBodyFactory = responseBodyFactory;
        }

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
            
            //TODO Move default content type to a global setting
            if (string.IsNullOrEmpty(response.ContentType))
            {
                response.ContentType = HttpContentTypes.ApplicationJson;
            }
        }

        private async Task WriteResponseBody(HttpResponse response, ResponseTemplate responseTemplate)
        {
            var responseBody = _responseBodyFactory.GetResponseBody(responseTemplate);

            await response.WriteAsync(responseBody);
        }
    }
}