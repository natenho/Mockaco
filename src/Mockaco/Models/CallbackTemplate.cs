using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Mockaco
{
    public class CallbackTemplate
    {
        public bool? Condition { get; set; }

        public string Method { get; set; }

        public string Url { get; set; }

        public int? Delay { get; set; }

        public int? Timeout { get; set; }

        public IDictionary<string, string> Headers { get; set; }

        public JContainer Body { get; set; }
    }
}