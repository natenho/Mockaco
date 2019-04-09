using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Mockaco.Processors;
using Mockaco.Routing;
using System;
using System.IO;
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
            foreach (var route in routerProvider.GetRoutes())
            {
                if (RouteMatchesRequest(httpContext.Request, route))
                {
                    scriptContext.AttachHttpContext(httpContext, route);

                    logger.LogDebug("Incoming request from {remoteIp}, {@request}", httpContext.Connection.RemoteIpAddress, scriptContext.Request.ToJson());
                    logger.LogInformation("Incoming request matches route {@route}", new { route.Method, route.Path, route.RawTemplate.Name });

                    var template = await templateTransformer.Transform(route.RawTemplate, scriptContext);

                    if (template.Request.Condition.GetValueOrDefault(true))
                    {
                        mockacoContext.Route = route;
                        mockacoContext.TransformedTemplate = template;

                        await _next(httpContext);
                        return;
                    }
                }
            }

            httpContext.Response.StatusCode = StatusCodes.Status501NotImplemented;
        }

        private bool RouteMatchesRequest(HttpRequest request, Route route)
        {
            if (!string.IsNullOrWhiteSpace(route.Method))
            {
                var methodMatches = request.Method.Equals(route.Method.ToString(), StringComparison.InvariantCultureIgnoreCase);
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

        private static string GetRawBody(HttpRequest httpRequest)
        {
            httpRequest.EnableRewind();

            using (var reader = new StreamReader(httpRequest.Body))
            {
                var json = reader.ReadToEnd();
                
                httpRequest.Body.Seek(0, SeekOrigin.Begin);

                return json;
            }
        }
    }
}