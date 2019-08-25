using System.Collections.Generic;

namespace Mockaco
{
    public class MockacoContext : IMockacoContext
    {
        public IScriptContext ScriptContext { get; set; }

        public Template TransformedTemplate { get; set; }

        public Mock Mock { get; set; }

        public List<Error> Errors { get; set; }
                
        public MockacoContext(IScriptContext scriptContext)
        {
            ScriptContext = scriptContext;
            Errors = new List<Error>();
        }
    }
}
