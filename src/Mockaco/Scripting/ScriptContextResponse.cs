using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Mockaco
{
    public class ScriptContextResponse
    {
        public IReadOnlyDictionary<string, string> Header { get; }

        public JContainer Body { get; }

        public ScriptContextResponse(
            PermissiveDictionary<string, string> header,
            JContainer body)
        {
            Header = header;
            Body = body;
        }
    }
}