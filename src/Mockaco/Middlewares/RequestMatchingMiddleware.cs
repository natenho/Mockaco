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

        public RequestMatchingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(
            HttpContext httpContext,
            IMockacoContext mockacoContext,
            IScriptContext scriptContext,
            IRouteProvider routerProvider,
            ITemplateTransformer templateTransformer,
            ILogger<RequestMatchingMiddleware> logger)
        {
            scriptContext.AttachHttpContext(httpContext);

            logger.LogInformation("Incoming request from {remoteIp}", httpContext.Connection.RemoteIpAddress);

            foreach (var route in routerProvider.GetRoutes())
            {
                if (RouteMatchesRequest(httpContext.Request, route))
                {
                    scriptContext.AttachRoute(httpContext, route);

                    logger.LogInformation("Incoming request matches route {route}", route);

                    var template = await templateTransformer.Transform(route.RawTemplate, scriptContext);

                    if (template.Request.Condition ?? true)
                    {
                        mockacoContext.Route = route;
                        mockacoContext.TransformedTemplate = template;

                        await _next(httpContext);

                        return;
                    }
                }
            }

            logger.LogInformation("Incoming request didn't match any route");

            httpContext.Response.StatusCode = StatusCodes.Status501NotImplemented;
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