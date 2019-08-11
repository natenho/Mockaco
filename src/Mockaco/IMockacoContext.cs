using Mockaco.Routing;

namespace Mockaco
{
    public interface IMockacoContext
    {
        IScriptContext ScriptContext { get; }

        Template TransformedTemplate { get; set; }

        Route Route { get; set; }
    }
}