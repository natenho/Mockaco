using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Mockaco
{
    public class ScriptContextResponse
    {
        public IReadOnlyDictionary<string, string> Header { get; }

        public JToken Body { get; }

        public ScriptContextResponse(StringDictionary header, JToken body)
        {
            Header = header;
            Body = body;
        }
    }
}