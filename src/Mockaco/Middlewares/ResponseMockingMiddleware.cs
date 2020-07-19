using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Net;
using System.Threading.Tasks;

namespace Mockaco
{
    public class ResponseMockingMiddleware
    {
        private readonly RequestDelegate _next;

        public ResponseMockingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(
            HttpContext httpContext,
            IMockacoContext mockacoContext,
            IScriptContext scriptContext,
            IResponseBodyFactory responseBodyFactory,
            IOptionsSnapshot<MockacoOptions> options)
        {
            await PrepareResponse(httpContext.Response, mockacoContext.TransformedTemplate, responseBodyFactory, options.Value);

            await scriptContext.AttachResponse(httpContext.Response, mockacoContext.TransformedTemplate.Response);

            await _next(httpContext);
        }

        private async Task PrepareResponse(
            HttpResponse httpResponse,
            Template transformedTemplate,
            IResponseBodyFactory responseBodyFactory,
            MockacoOptions options)
        {
            httpResponse.StatusCode = GetResponseStatusFromTemplate(transformedTemplate.Response, options);

            AddHeadersFromTemplate(httpResponse, transformedTemplate.Response, options);

            var bodyBytes = await responseBodyFactory.GetResponseBodyBytesFromTemplate(transformedTemplate.Response);

            if (bodyBytes == default)
            {
                return;
            }

            await httpResponse.Body.WriteAsync(bodyBytes, 0, bodyBytes.Length, default);
        }

        private int GetResponseStatusFromTemplate(ResponseTemplate responseTemplate, MockacoOptions options)
        {
            return responseTemplate?.Status == default(HttpStatusCode)
                            ? (int)options.DefaultHttpStatusCode
                            : (int)responseTemplate.Status;
        }

        private void AddHeadersFromTemplate(HttpResponse response, ResponseTemplate responseTemplate, MockacoOptions options)
        {
            if (responseTemplate?.Headers != null)
            {
                foreach (var header in responseTemplate.Headers)
                {
                    response.Headers.Add(header.Key, header.Value);
                }
            }

            if (string.IsNullOrEmpty(response.ContentType))
            {
                response.ContentType = options.DefaultHttpContentType;
            }
        }
    }
}
