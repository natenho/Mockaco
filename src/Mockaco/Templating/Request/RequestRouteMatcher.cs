using Microsoft.AspNetCore.Http;

namespace Mockaco
{
    public class RequestRouteMatcher : IRequestMatcher
    {
        public bool IsMatch(HttpRequest httpRequest, Mock mock)
        {
            if (string.IsNullOrWhiteSpace(mock?.Route))
            {
                return false;
            }

            var routeMatcher = new RouteMatcher();
            var isMatch = routeMatcher.IsMatch(mock.Route, httpRequest.Path);

            return isMatch;
        }
    }
}
