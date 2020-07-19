using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
            ITemplateTransformer templateTransformer,
            IEnumerable<IRequestMatcher> requestMatchers
            )
        {
            await LogHttpContext(httpContext);

            AttachRequestToScriptContext(httpContext, mockacoContext, scriptContext);

            if (mockacoContext.Errors.Any())
            {
                return;
            }

            foreach (var mock in mockProvider.GetMocks())
            {
                if (requestMatchers.All(_ => _.IsMatch(httpContext.Request, mock)))
                {
                    await scriptContext.AttachRouteParameters(httpContext.Request, mock);

                    var template = await templateTransformer.Transform(mock.RawTemplate, scriptContext);

                    var conditionIsMatch = template.Request?.Condition ?? true;

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

        private async Task LogHttpContext(HttpContext httpContext)
        {
            _logger.LogInformation("Incoming request from {remoteIp}", httpContext.Connection.RemoteIpAddress);

            _logger.LogDebug("Headers: {headers}", httpContext.Request.Headers.ToJson());

            var body = await httpContext.Request.ReadBodyStream();

            if (string.IsNullOrEmpty(body))
            {
                _logger.LogDebug("Body is not present", body);
            }
            else
            {
                _logger.LogDebug("Body: {body}", body);
            }
        }
    }
}