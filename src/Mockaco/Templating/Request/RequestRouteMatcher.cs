using Microsoft.AspNetCore.Http;

namespace Mockaco
{
    public class RequestRouteMatcher : IRequestMatcher
    {
        private const string DefaultRoute = "/";

        public bool IsMatch(HttpRequest httpRequest, Mock mock)
        {
            var routeMatcher = new RouteMatcher();

            if (string.IsNullOrWhiteSpace(mock?.Route))
            {
                return routeMatcher.IsMatch(DefaultRoute, httpRequest.Path);
            }

            return routeMatcher.IsMatch(mock.Route, httpRequest.Path);
        }
    }
}
