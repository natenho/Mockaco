using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            ITemplateTransformer templateTransformer,
            IOptionsSnapshot<StatusCodesOptions> statusCodesOptions
            )
        {
            try
            {
                scriptContext.AttachRequest(httpContext.Request);
            }
            catch (Exception ex)
            {
                mockacoContext.Errors.Add(new Error("An error occurred while reading request", ex));

                return;
            }

            LogHttpContext(httpContext);

            foreach (var route in routerProvider.GetRoutes())
            {
                if (RouteMatchesRequest(httpContext.Request, route))
                {
                    scriptContext.AttachRoute(httpContext.Request, route);

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
                        _logger.LogDebug("Incoming request didn't match condition for route {route}", route);
                    }
                }
                else
                {
                    _logger.LogDebug("Incoming request didn't match route {route}", route);
                }
            }

            _logger.LogInformation("Incoming request didn't match any route");

            mockacoContext.Errors.Add(new Error("Incoming request didn't match any route"));
        }

        private void LogHttpContext(HttpContext httpContext)
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