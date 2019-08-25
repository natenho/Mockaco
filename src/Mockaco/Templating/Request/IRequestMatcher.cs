using Microsoft.AspNetCore.Http;

namespace Mockaco
{
    public interface IRequestMatcher
    {
        bool IsMatch(HttpRequest httpRequest, Mock mock);
    }
}