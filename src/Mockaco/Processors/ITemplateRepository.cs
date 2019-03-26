using System.Collections.Generic;

namespace Mockaco
{
    public interface ITemplateRepository
    {
        IEnumerable<TemplateFile> GetAll();
    }
}