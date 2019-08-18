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
            IMockProvider mockProvider,
            ITemplateTransformer templateTransformer
            )
        {
            AttachRequestToScriptContext(httpContext, mockacoContext, scriptContext);

            LogHttpContext(httpContext);

            foreach (var mock in mockProvider.GetMocks())
            {
                if (MockMatchesRequest(httpContext.Request, mock))
                {
                    scriptContext.AttachRouteParameters(httpContext.Request, mock);

                    var template = await templateTransformer.Transform(mock.RawTemplate, scriptContext);

                    var matchesCondition = template.Request.Condition ?? true;

                    if (matchesCondition)
                    {
                        _logger.LogInformation("Incoming request matched {mock}", mock);

                        mockacoContext.Mock = mock;
                        mockacoContext.TransformedTemplate = template;

                        await _next(httpContext);

                        return;
                    }
                    else
                    {
                        _logger.LogDebug("Incoming request didn't match condition for {mock}", mock);
                    }
                }
                else
                {
                    _logger.LogDebug("Incoming request didn't match {mock}", mock);
                }
            }

            _logger.LogInformation("Incoming request didn't match any mock");

            mockacoContext.Errors.Add(new Error("Incoming request didn't match any mock"));
        }

        private static void AttachRequestToScriptContext(HttpContext httpContext, IMockacoContext mockacoContext, IScriptContext scriptContext)
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

        private static bool MockMatchesRequest(HttpRequest request, Mock mock)
        {
            if (!string.IsNullOrWhiteSpace(mock.Method))
            {
                var methodMatches = request.Method.Equals(mock.Method, StringComparison.InvariantCultureIgnoreCase);
                if (!methodMatches)
                {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(mock.Route))
            {
                var routeMatcher = new RouteMatcher();
                var routeMatches = routeMatcher.IsMatch(mock.Route, request.Path);
                if (!routeMatches)
                {
                    return false;
                }
            }

            return true;
        }
    }
}