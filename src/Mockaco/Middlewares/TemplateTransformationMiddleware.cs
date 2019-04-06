using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Mockaco.Processors;
using Mono.TextTemplating;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Mockaco.Middlewares
{
    public class TemplateTransformationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TemplateTransformationMiddleware> _logger;

        public TemplateTransformationMiddleware(RequestDelegate next, ILogger<TemplateTransformationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext, MockacoContext mockacoContext, ITemplateRepository templateRepository, ITemplateTransformer _templateTransformer)
        {
            mockacoContext.ScriptContext = new ScriptContext(httpContext);

            foreach (var templateFile in templateRepository.GetAll())
            {
                try
                {
                    var transformedTemplate = await _templateTransformer.Transform(templateFile.Content, mockacoContext.ScriptContext);
                    var template = JsonConvert.DeserializeObject<Template>(transformedTemplate);
                    mockacoContext.AvailableTemplates.Add(template);
                }
                catch (JsonReaderException ex)
                {
                    _logger.LogWarning("Skipping {0}: Generated JSON is invalid - {1}", templateFile.FileName, ex.Message);
                }
                catch (ParserException ex)
                {
                    _logger.LogWarning("Skipping {0}: Script parser error - {1} {2} ", templateFile.FileName, ex.Message, ex.Location);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Skipping {0}: {1}", templateFile.FileName, ex.Message);
                }
            }

            await _next(httpContext);
        }
    }
}
