using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mockaco
{
    public interface ITemplateProvider
    {
        event EventHandler OnChange;

        Task<IEnumerable<RawTemplate>> GetTemplates();
    }
}