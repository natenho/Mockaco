using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Mockaco
{
    public interface IRequestBodyStrategy
    {
        bool CanHandle(HttpRequest httpRequest);
        Task<JToken> ReadBodyAsJson(HttpRequest httpRequest);
    }
}