using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;

namespace Mockaco
{
    public class ResponseTemplate
    {
        public IDictionary<string, string> Headers { get; set; }
        public int Delay { get; set; }        
        public HttpStatusCode Status { get; set; }
        public JContainer Body { get; set; }
    }
}