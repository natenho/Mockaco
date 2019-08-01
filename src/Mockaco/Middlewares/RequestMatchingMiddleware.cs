using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Mockaco.Processors;
using Mockaco.Routing;
using System;
using System.Threading.Tasks;

namespace Mockaco.Middlewares
{
    public class RequestMatchingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestMatchingMiddleware> _logger;

        public RequestMatchingMiddleware(RequestDelegate next, ILogger<RequestMatchingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(
            HttpContext httpContext,
            IMockacoContext mockacoContext,
            IScriptContext scriptContext,
            IRouteProvider routerProvider,
            ITemplateTransformer templateTransformer
            )
        {
            scriptContext.AttachHttpContext(httpContext);

            LogRequest(httpContext);

            foreach (var route in routerProvider.GetRoutes())
            {
                if (RouteMatchesRequest(httpContext.Request, route))
                {
                    scriptContext.AttachRoute(httpContext, route);

                    var template = await templateTransformer.Transform(route.RawTemplate, scriptContext);

                    var matchesCondition = template.Request.Condition ?? true;

                    if (matchesCondition)
                    {
                        _logger.LogInformation("Incoming request matched route {route}", route);

                        mockacoContext.Route = route;
                        mockacoContext.TransformedTemplate = template;

                        await _next(httpContext);

                        return;
                    }
                    else
                    {
                        _logger.LogInformation("Incoming request didn't match condition for route {route}", route);
                    }
                }
                else
                {
                    _logger.LogDebug("Incoming request didn't match route {route}", route);
                }
            }

            _logger.LogInformation("Incoming request didn't match any route");

            httpContext.Response.StatusCode = StatusCodes.Status501NotImplemented;
        }

        private void LogRequest(HttpContext httpContext)
        {
            _logger.LogInformation("Incoming request from {remoteIp}", httpContext.Connection.RemoteIpAddress);

            _logger.LogDebug("Headers: {headers}", httpContext.Request.Headers.ToJson());

            var body = httpContext.Request.ReadBodyStream();

            if (string.IsNullOrEmpty(body))
            {
                _logger.LogDebug("Body is not present", body);
            }
            else
            {
                _logger.LogDebug("Body: {body}", body);
            }
        }

        private static bool RouteMatchesRequest(HttpRequest request, Route route)
        {
            if (!string.IsNullOrWhiteSpace(route.Method))
            {
                var methodMatches = request.Method.Equals(route.Method, StringComparison.InvariantCultureIgnoreCase);
                if (!methodMatches)
                {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(route.Path))
            {
                var routeMatcher = new RouteMatcher();
                var routeMatches = routeMatcher.IsMatch(route.Path, request.Path);
                if (!routeMatches)
                {
                    return false;
                }
            }

            return true;
        }
    }
}