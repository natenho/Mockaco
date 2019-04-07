using System;
using System.Collections.Generic;

namespace Mockaco
{
    public interface ITemplateProvider
    {
        event EventHandler OnChange;

        IEnumerable<IRawTemplate> GetTemplates();
    }
}