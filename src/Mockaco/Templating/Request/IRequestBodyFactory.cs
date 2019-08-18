using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace Mockaco
{
    public interface IRequestBodyFactory
    {
        JObject ReadBodyAsJson(HttpRequest httpRequest);
    }
}
