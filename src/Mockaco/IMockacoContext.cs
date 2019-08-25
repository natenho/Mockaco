using System.Collections.Generic;

namespace Mockaco
{
    public interface IMockacoContext
    {
        IScriptContext ScriptContext { get; }

        Template TransformedTemplate { get; set; }

        Mock Mock { get; set; }

        List<Error> Errors { get; set; }
    }
}