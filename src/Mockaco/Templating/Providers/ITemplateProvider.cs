using System;
using System.Collections.Generic;

namespace Mockaco
{
    public interface ITemplateProvider
    {
        IEnumerable<IRawTemplate> GetTemplates();
    }
}