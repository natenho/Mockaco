using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Mockaco
{
    public class ScriptContextRequest
    {
        public Uri Url { get; }

        public IReadOnlyDictionary<string, string> Route { get; }

        public IReadOnlyDictionary<string, string> Query { get; }

        public IReadOnlyDictionary<string, string> Header { get; }

        public JToken Body { get; }

        public ScriptContextRequest(
            Uri url,
            IReadOnlyDictionary<string, string> route,
            IReadOnlyDictionary<string, string> query,
            IReadOnlyDictionary<string, string> header,
            JToken body)
        {
            Url = url;
            Route = route;
            Query = query;
            Header = header;
            Body = body;
        }      
    }
}