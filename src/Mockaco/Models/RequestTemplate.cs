using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace Mockore
{
    public class RequestTemplate
    {
        public HttpMethod? Method { get; set; }
        public string Route { get; set; }
        public string Condition { get; set; }
    }
}