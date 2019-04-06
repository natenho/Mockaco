using System.Collections.Generic;

namespace Mockaco
{
    public class ScriptContextResponse
    {
        public IReadOnlyDictionary<string, string> Header { get; }

        public PermissiveJraw Body { get; }

        public ScriptContextResponse(
            PermissiveDictionary<string, string> header,
            PermissiveJraw body)
        {
            Header = header;
            Body = body;
        }
    }
}