using System.Collections.Generic;

namespace Mockore
{
    public interface ITemplateRepository
    {
        IEnumerable<Template> GetAll();
    }
}