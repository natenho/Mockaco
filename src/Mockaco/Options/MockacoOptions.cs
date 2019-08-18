using System.Net;

namespace Mockaco
{
    public class MockacoOptions
    {
        public HttpStatusCode DefaultHttpStatusCode { get; set; }

        public HttpStatusCode ErrorHttpStatusCode { get; set; }       

        public string DefaultHttpContentType { get; set; }

        public MockacoOptions()
        {
            DefaultHttpStatusCode = HttpStatusCode.OK;
            ErrorHttpStatusCode = HttpStatusCode.NotImplemented;
            DefaultHttpContentType = HttpContentTypes.ApplicationJson;
        }
    }
}
