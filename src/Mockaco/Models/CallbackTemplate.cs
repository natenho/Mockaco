using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;

namespace Mockaco
{
    public class CallbackTemplate
    {
        public string Method { get; set; }
        public string Url { get; set; }
        public string Delay { get; set; }
        public string Timeout { get; set; }
        public IDictionary<string, string> Headers { get; set; }
        public JRaw Body { get; set; }
    }
}