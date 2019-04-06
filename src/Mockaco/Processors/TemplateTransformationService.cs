using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Mockaco.Processors;
using Mono.TextTemplating;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Mockaco.Middlewares
{
    public class TemplateTransformationService
    {
        private readonly ITemplateRepository _templateRepository;
        private readonly ITemplateTransformer _templateTransformer;
        private readonly ILogger<TemplateTransformationService> _logger;

        public TemplateTransformationService(ITemplateRepository templateRepository, ITemplateTransformer templateTransformer, ILogger<TemplateTransformationService> logger)
        {
            _templateRepository = templateRepository;
            _templateTransformer = templateTransformer;
            _logger = logger;
        }

        public async Task Process(HttpContext httpContext, MockacoContext mockacoContext)
        {
            mockacoContext.AttachHttpContext(httpContext);

            foreach (var templateFile in _templateRepository.GetAll())
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
        }

        public Task WarmUp()
        {
            return Process(null, new MockacoContext());
        }
    }
}
