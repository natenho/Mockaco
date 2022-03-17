using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Mockaco
{
    internal interface IRequestMatcher
    {
        Task<bool> IsMatch(HttpRequest httpRequest, Mock mock);
    }
}