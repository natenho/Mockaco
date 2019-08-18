using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace Mockaco
{
    public interface IRequestBodyStrategy
    {
        bool CanHandle(HttpRequest httpRequest);
        JObject ReadBodyAsJson(HttpRequest httpRequest);
    }
}