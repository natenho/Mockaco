using Microsoft.OpenApi.Readers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Mockaco.Generation.Converters.OpenApi
{
    public class OpenApiSourceConverter
    {
        public Task<ConvertionResult> Convert(Stream contentStream)
        {
            var openApiDocument = new OpenApiStreamReader().Read(contentStream, out var diagnostic);
            var templates = new List<Template>();

            foreach (var kvPath in openApiDocument.Paths)
            {
                var path = kvPath.Key;
                foreach (var kvOperation in kvPath.Value.Operations)
                {
                    var type = kvOperation.Key;
                    var operation = kvOperation.Value;

                    var template = new Template
                    {
                        Request = new RequestTemplate
                        {
                            Route = path,
                            Method = type.ToString().ToUpper(),
                        },
                        
                    };

                    templates.Add(template);
                }
            }

            return Task.FromResult(new ConvertionResult
            {
                RawTemplates = templates
            });
        }
    }
}
