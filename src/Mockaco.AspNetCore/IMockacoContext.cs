using System.Collections.Generic;

namespace Mockaco
{
    internal interface IMockacoContext
    {
        IScriptContext ScriptContext { get; }

        Template TransformedTemplate { get; set; }

        Mock Mock { get; set; }

        List<Error> Errors { get; set; }
    }
}