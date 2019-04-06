using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Mockaco.Processors;
using Mockaco.Routing;

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
            ITemplateTransformer templateTransformer)
        {
            foreach (var route in routerProvider.GetRoutes())
            {
                if (RouteMatchesRequest(httpContext.Request, route))
                {                    
                    scriptContext.AttachHttpContext(httpContext, route);

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
    }
}
