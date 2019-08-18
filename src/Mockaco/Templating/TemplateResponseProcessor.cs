using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Mockaco
{
    public class TemplateResponseProcessor : ITemplateResponseProcessor
    {   
        private readonly IResponseBodyFactory _responseBodyFactory;
        private readonly MockacoOptions _options;

        public TemplateResponseProcessor(IResponseBodyFactory responseBodyFactory, IOptionsSnapshot<MockacoOptions> options)
        {
            _responseBodyFactory = responseBodyFactory;
            _options = options.Value;
        }

        public async Task PrepareResponse(HttpResponse httpResponse, IScriptContext scriptContext, ResponseTemplate responseTemplate)
        {
            httpResponse.StatusCode = responseTemplate.Status == default
                ? (int)_options.DefaultHttpStatusCode
                : (int)responseTemplate.Status;

            AddHeaders(httpResponse, responseTemplate);

            var body = _responseBodyFactory.GetResponseBody(responseTemplate);

            await httpResponse.WriteAsync(body);
                      
            scriptContext.AttachResponse(httpResponse.Headers, responseTemplate.Body);
        }

        private void AddHeaders(HttpResponse response, ResponseTemplate responseTemplate)
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
                response.ContentType = _options.DefaultHttpContentType;
            }
        }
    }
}