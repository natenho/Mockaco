using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Mockaco
{
    public interface IRequestMatcher
    {
        Task<bool> IsMatch(HttpRequest httpRequest, Mock mock);
    }
}