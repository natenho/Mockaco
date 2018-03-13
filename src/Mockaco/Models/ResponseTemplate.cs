using Newtonsoft.Json.Linq;
using System.Net;

namespace Mockore
{
    public class ResponseTemplate
    {
        public int Delay { get; set; }
        public HttpStatusCode Status { get; set; }
        public JContainer Body { get; set; }
    }
}