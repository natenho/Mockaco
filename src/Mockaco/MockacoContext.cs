using Mockaco.Routing;

namespace Mockaco
{
    public class MockacoContext : IMockacoContext
    {
        public IScriptContext ScriptContext { get; set; }
        public Template TransformedTemplate { get; set; }
        public Route Route { get; set; }

        public MockacoContext()
        {
            ScriptContext = new ScriptContext();
        }
    }
}
