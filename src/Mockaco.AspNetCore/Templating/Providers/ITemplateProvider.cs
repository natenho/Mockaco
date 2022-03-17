using System;
using System.Collections.Generic;

namespace Mockaco
{
    internal interface ITemplateProvider
    {
        event EventHandler OnChange;

        IEnumerable<IRawTemplate> GetTemplates();
    }
}