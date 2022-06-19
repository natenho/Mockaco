using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Newtonsoft.Json.Linq;

namespace Mockaco.Templating.Generating.Providers
{
    internal class OpenApiTemplateProvider : IGeneratedTemplateProvider
    {
        private readonly ILogger _logger;

        public OpenApiTemplateProvider(ILogger<OpenApiTemplateProvider> logger)
        {
            _logger = logger;
        }
        
        public Task<IEnumerable<GeneratedTemplate>> GetTemplatesAsync(Stream sourceStream, CancellationToken cancellationToken = default)
        {
            
            var openApiDocument = new OpenApiStreamReader(
                    new OpenApiReaderSettings
                    {
                        ReferenceResolution = ReferenceResolutionSetting.ResolveLocalReferences
                    }
                )
                .Read(sourceStream, out var diagnostic);
            
            LogDiagnostics(diagnostic);

            return Task.FromResult(BuildTemplates(openApiDocument));
        }

        private IEnumerable<GeneratedTemplate> BuildTemplates(OpenApiDocument openApiDocument)
        {
            foreach (var path in openApiDocument.Paths)
            {
                foreach (var operation in path.Value.Operations)
                {
                    var template = new GeneratedTemplate
                    {
                        Request = BuildRequest(path, operation),
                        Response = BuildResponse(operation)
                    };

                    SetTemplateName(template, operation);
                    yield return template;
                }
            }
        }

        private void SetTemplateName(GeneratedTemplate template, KeyValuePair<OperationType, OpenApiOperation> operation)
        {
            template.Name = operation.Value.OperationId;
            if (string.IsNullOrWhiteSpace(template.Name))
            {
                template.Name = Guid.NewGuid().ToString("N");
                _logger.LogWarning(
                    "The OperationId for request {Method} {Route} hasn't been declared. Using generated template name: {TemplateName}",
                    template.Request.Method, template.Request.Route, template.Name);
            }
        }

        private ResponseTemplate BuildResponse(KeyValuePair<OperationType,OpenApiOperation> operation)
        {
            var responseTemplate = new ResponseTemplate();
            if (operation.Value.Responses.Any())
            {
                var response = operation.Value.Responses.First();
                responseTemplate.Status = (HttpStatusCode) int.Parse(response.Key);
                if (response.Value.Content.Any())
                {
                    var content = response.Value.Content.First();
                    responseTemplate.Headers.Add("Content-Type", content.Key);
                    if (content.Value.Example != null)
                    {
                        responseTemplate.Body = JValue.CreateString(content.Value.Example.ToString());
                    }
                }
            }
            else
            {
                responseTemplate.Status = HttpStatusCode.NotFound;
            }
            return responseTemplate;
        }

        private RequestTemplate BuildRequest(KeyValuePair<string,OpenApiPathItem> path, KeyValuePair<OperationType,OpenApiOperation> operation)
        {
            return new RequestTemplate
            {
                Route = path.Key,
                Method = Enum.GetName(operation.Key)?.ToUpper()
            };
        }

        private void LogDiagnostics(OpenApiDiagnostic diagnostic)
        {
            _logger.LogDebug("OpenApi specification version: {OpenApiSpecVersion}", Enum.GetName(diagnostic.SpecificationVersion));
            foreach (var diagnosticError in diagnostic.Errors)
            {
                _logger.LogWarning("OpenApi parser diagnostic error: {Error}", diagnosticError);
            }
        }
    }
}