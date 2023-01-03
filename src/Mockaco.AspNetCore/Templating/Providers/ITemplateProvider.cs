using System;
using System.Collections.Generic;

namespace Mockaco
{
    internal interface ITemplateProvider
    {
        event EventHandler OnChange;

        IEnumerable<IRawTemplate> GetTemplates();
        void Remove(string name);
        void Update(string name, string content);
    }
}