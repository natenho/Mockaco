using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Mockaco
{
    //TODO Implement Swagger importer
    public sealed class TemplateRepository : ITemplateRepository
    {
        private readonly List<Template> _templates = new List<Template>();

        public TemplateRepository()
        {
            var directory = new DirectoryInfo("Mocks");
            foreach (var file in directory.GetFiles("*.json", SearchOption.AllDirectories))
            {
                var value = File.ReadAllText(file.FullName);
                _templates.Add(JsonConvert.DeserializeObject<Template>(value));
            }
        }

        public IEnumerable<Template> GetAll()
        {
            return _templates;
        }
    }
}