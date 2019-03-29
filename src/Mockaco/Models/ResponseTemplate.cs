using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;

namespace Mockaco
{
    public class ResponseTemplate
    {        
        public string Delay { get; set; }        
        public HttpStatusCode Status { get; set; }
        public IDictionary<string, string> Headers { get; set; }
        public JRaw Body { get; set; }
    }
}