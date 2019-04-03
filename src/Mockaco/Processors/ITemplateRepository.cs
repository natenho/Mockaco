using System;
using System.Collections.Generic;

namespace Mockaco
{
    public interface ITemplateRepository
    {
        event EventHandler CacheInvalidated; // TODO Wrong place for this event
        IEnumerable<TemplateFile> GetAll();
    }
}