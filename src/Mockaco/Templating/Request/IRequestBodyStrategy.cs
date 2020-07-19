using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Mockaco
{
    public interface IRequestBodyStrategy
    {
        bool CanHandle(HttpRequest httpRequest);
        Task<JObject> ReadBodyAsJson(HttpRequest httpRequest);
    }
}