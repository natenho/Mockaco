using System.Collections.Generic;
using System.IO;

namespace Mockaco
{
    //TODO Implement Swagger importer
    public sealed class TemplateRepository : ITemplateRepository
    {
        private readonly List<TemplateFile> _templates = new List<TemplateFile>();

        public TemplateRepository()
        {
            var directory = new DirectoryInfo("Mocks");
            foreach (var file in directory.GetFiles("*.json"))
            {
                var rawContent = File.ReadAllText(file.FullName);
                _templates.Add(new TemplateFile(file.Name, rawContent));
            }
        }

        public IEnumerable<TemplateFile> GetAll()
        {
            return _templates;
        }
    }
}