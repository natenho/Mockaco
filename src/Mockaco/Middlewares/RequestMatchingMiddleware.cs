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

            await AttachRequestToScriptContext(httpContext, mockacoContext, scriptContext);

            if (mockacoContext.Errors.Any())
            {
                return;
            }

            foreach (var mock in mockProvider.GetMocks())
            {
                if (await requestMatchers.AllAsync(_ => _.IsMatch(httpContext.Request, mock)))
                {                    
                    _logger.LogInformation("Incoming request matched {mock}", mock);

                    await scriptContext.AttachRouteParameters(httpContext.Request, mock);

                    var template = await templateTransformer.TransformAndSetVariables(mock.RawTemplate, scriptContext);

                    mockacoContext.Mock = mock;
                    mockacoContext.TransformedTemplate = template;

                    await _next(httpContext);

                    return;
                }
                else
                {
                    _logger.LogTrace("Incoming request didn't match {mock}", mock);
                }
            }

            _logger.LogInformation("Incoming request didn't match any mock");

            mockacoContext.Errors.Add(new Error("Incoming request didn't match any mock"));
        }

        //TODO Remove redundant code
        private async Task AttachRequestToScriptContext(HttpContext httpContext, IMockacoContext mockacoContext, IScriptContext scriptContext)
        {
            try
            {
                await scriptContext.AttachRequest(httpContext.Request);
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