using Mockaco.Routing;
using System.Collections.Generic;

namespace Mockaco
{
    public interface IMockacoContext
    {
        IScriptContext ScriptContext { get; }

        Template TransformedTemplate { get; set; }

        Route Route { get; set; }

        List<Error> Errors { get; set; }
    }
}