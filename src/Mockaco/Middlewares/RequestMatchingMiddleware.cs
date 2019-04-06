using Microsoft.AspNetCore.Http;
using Mockaco.Processors;
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

        public async Task Invoke(HttpContext httpContext, MockacoContext mockacoContext, ITemplateRepository templateRepository, ITemplateTransformer _templateTransformer)
        {
            foreach (var template in mockacoContext.AvailableTemplates)
            {
                if (RequestMatchesTemplate(httpContext, template.Request, mockacoContext.ScriptContext))
                {
                    mockacoContext.Template = template;
                    await _next(httpContext);
                }
            }
        }

        private bool RequestMatchesTemplate(HttpContext httpContext, RequestTemplate requestTemplate, ScriptContext scriptContext)
        {
            if (requestTemplate.Method != null)
            {
                var methodMatches = httpContext.Request.Method.Equals(requestTemplate.Method.ToString(), StringComparison.InvariantCultureIgnoreCase);
                if (!methodMatches)
                {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(requestTemplate.Route))
            {
                var routeMatcher = new RouteMatcher();
                var routeMatches = routeMatcher.IsMatch(requestTemplate.Route, httpContext.Request.Path);
                if (!routeMatches)
                {
                    return false;
                }
            }

            return requestTemplate.Condition.GetValueOrDefault(true);
        }
    }
}
