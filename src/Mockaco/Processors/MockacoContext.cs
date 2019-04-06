using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Mockaco
{
    public class MockacoContext
    {
        public ScriptContext ScriptContext { get; set; }
        public List<Template> AvailableTemplates { get; set; }
        public Template Template { get; set; }

        public MockacoContext()
        {
            AvailableTemplates = new List<Template>();
            ScriptContext = new ScriptContext();
        }

        public void AttachHttpContext(HttpContext httpContext)
        {
            if(httpContext == null)
            {
                return;
            }

            ScriptContext.AttachHttpContext(httpContext);
        }
    }
}
