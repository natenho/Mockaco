using System.Collections.Generic;
using System.Net;

namespace Mockaco
{
    public class MockacoOptions
    {
        public HttpStatusCode DefaultHttpStatusCode { get; set; }

        public HttpStatusCode ErrorHttpStatusCode { get; set; }       

        public string DefaultHttpContentType { get; set; }

        public List<string> References { get; set; }

        public List<string> Imports { get; set; }

        public int MatchedRoutesCacheDuration { get; set; }

        public MockacoOptions()
        {
            DefaultHttpStatusCode = HttpStatusCode.OK;
            ErrorHttpStatusCode = HttpStatusCode.NotImplemented;
            DefaultHttpContentType = HttpContentTypes.ApplicationJson;
            References = new List<string>();
            Imports = new List<string>();
        }
    }
}
