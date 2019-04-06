using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace Mockaco
{
    public sealed class TemplateFileProvider : ITemplateProvider
    {
        private readonly List<RawTemplate> _templates = new List<RawTemplate>();
        
        private readonly ILogger<TemplateFileProvider> _logger;

        public TemplateFileProvider(ILogger<TemplateFileProvider> logger)
        {
            _logger = logger;
        }

        public IEnumerable<IRawTemplate> GetTemplates()
        {
            var directory = new DirectoryInfo("Mocks");
            foreach (var file in directory.GetFiles("*.json"))
            {
                var rawContent = File.ReadAllText(file.FullName);
                yield return new RawTemplate(file.Name, rawContent);
            }
        }
    }
}