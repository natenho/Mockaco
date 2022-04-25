using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mockaco.Templating.Generating.Store
{
    public class GeneratedTemplateStore : IGeneratedTemplateStore
    {
        private const string FileExtension = ".json";
        
        private readonly IOptions<TemplateStoreOptions> _options;
        private readonly ILogger _logger;

        public GeneratedTemplateStore(IOptions<TemplateStoreOptions> options, ILogger<GeneratedTemplateStore> logger)
        {
            _options = options;
            _logger = logger;
        }

        private string RootDir => _options.Value.RootDir;
        
        public async Task SaveAsync(GeneratedTemplate template, CancellationToken cancellationToken = default)
        {
            var templateFileName = GetFileName(template);
            Directory.CreateDirectory(Path.GetDirectoryName(templateFileName) ?? string.Empty);
            await File.WriteAllBytesAsync(templateFileName, GetTemplateContent(template), cancellationToken);

            _logger.LogInformation("Generated {template} for {method} {route}", templateFileName, template.Request.Method, template.Request.Route);
        }

        private byte[] GetTemplateContent(GeneratedTemplate template)
        {
            return JsonSerializer.SerializeToUtf8Bytes<Template>(template, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                IgnoreNullValues = true,
            });
        }

        public async Task SaveAsync(IEnumerable<GeneratedTemplate> templates, CancellationToken cancellationToken = default)
        {
            foreach (var template in templates)
            {
                if (!cancellationToken.IsCancellationRequested)
                    await SaveAsync(template, cancellationToken);
                else break;
            }
        }

        private string GetFileName(GeneratedTemplate template)
        {
            var fileName = template.Name;
            if (!fileName.EndsWith(FileExtension))
            {
                fileName += FileExtension;
            }
            
            return fileName.StartsWith(RootDir) ? fileName : Path.Combine(RootDir, fileName);
        }
    }
}