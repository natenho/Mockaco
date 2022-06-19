using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Mockaco
{
    public interface IRequestBodyFactory
    {
        Task<JToken> ReadBodyAsJson(HttpRequest httpRequest);
    }
}
