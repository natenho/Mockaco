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
            logger.LogDebug("Headers: {headers}", scriptContext.Request.Header.ToJson());
            logger.LogDebug("Body: {body}", scriptContext.Request.Body.ToString());

            foreach (var route in routerProvider.GetRoutes())
            {
                if (RouteMatchesRequest(httpContext.Request, route))
                {
                    scriptContext.AttachRoute(httpContext, route);
                    
                    var template = await templateTransformer.Transform(route.RawTemplate, scriptContext);

                    var evaluatedCondition = template.Request.Condition ?? true;
                                        
                    if (evaluatedCondition)
                    {
                        logger.LogInformation("Incoming request matches route {route}", route);

                        mockacoContext.Route = route;
                        mockacoContext.TransformedTemplate = template;

                        await _next(httpContext);

                        return;
                    }
                    else
                    {
                        logger.LogInformation("Incoming request didn't match condition for route {route}", route);
                    }
                }
                else
                {
                    logger.LogDebug("Incoming request didn't match route {route}", route);
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