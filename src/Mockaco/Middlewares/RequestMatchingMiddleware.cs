using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mockaco
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
            LogHttpContext(httpContext);

            AttachRequestToScriptContext(httpContext, mockacoContext, scriptContext);

            if(mockacoContext.Errors.Any())
            {
                return;
            }                       

            foreach (var mock in mockProvider.GetMocks())
            {
                if (RequestMatchesMock(httpContext.Request, mock))
                {
                    scriptContext.AttachRouteParameters(httpContext.Request, mock);

                    var template = await templateTransformer.Transform(mock.RawTemplate, scriptContext);

                    var conditionIsMatch = template.Request.Condition ?? true;

                    if (conditionIsMatch)
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

        private void AttachRequestToScriptContext(HttpContext httpContext, IMockacoContext mockacoContext, IScriptContext scriptContext)
        {
            try
            {
                scriptContext.AttachRequest(httpContext.Request);
            }
            catch (Exception ex)
            {
                mockacoContext.Errors.Add(new Error("An error occurred while reading request", ex));

                _logger.LogWarning(ex, "An error occurred while reading request");

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

        private static bool RequestMatchesMock(HttpRequest request, Mock mock)
        {
            if (!string.IsNullOrWhiteSpace(mock.Method))
            {
                var methodIsMatch = request.Method.Equals(mock.Method, StringComparison.InvariantCultureIgnoreCase);
                if (!methodIsMatch)
                {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(mock.Route))
            {
                var routeMatcher = new RouteMatcher();
                var routeIsMatch = routeMatcher.IsMatch(mock.Route, request.Path);
                if (!routeIsMatch)
                {
                    return false;
                }
            }

            return true;
        }
    }
}