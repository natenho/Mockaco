using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Mockaco
{
    public class RequestRouteMatcher : IRequestMatcher
    {
        private const string DefaultRoute = "/";

        public Task<bool> IsMatch(HttpRequest httpRequest, Mock mock)
        {
            var routeMatcher = new RouteMatcher();

            return Task.FromResult(string.IsNullOrWhiteSpace(mock?.Route) ? 
                routeMatcher.IsMatch(DefaultRoute, httpRequest.Path, httpRequest.Query) : 
                routeMatcher.IsMatch(mock.Route, httpRequest.Path, httpRequest.Query));
        }
    }
}
