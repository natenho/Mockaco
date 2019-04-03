using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace Mockaco
{
    public sealed class TemplateFileRepository : ITemplateRepository
    {
        private readonly List<TemplateFile> _templates = new List<TemplateFile>();
        
        private readonly ILogger<TemplateFileRepository> _logger;

        public TemplateFileRepository(ILogger<TemplateFileRepository> logger)
        {
            _logger = logger;
        }

        public event EventHandler CacheInvalidated;

        public IEnumerable<TemplateFile> GetAll()
        {
            var directory = new DirectoryInfo("Mocks");
            foreach (var file in directory.GetFiles("*.json"))
            {
                var rawContent = File.ReadAllText(file.FullName);
                yield return new TemplateFile(file.Name, rawContent);
            }
        }
    }
}