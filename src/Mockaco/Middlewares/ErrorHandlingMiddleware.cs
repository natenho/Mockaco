using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mockaco.Routing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mockaco
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(
            HttpContext httpContext,
            IMockacoContext mockacoContext,
            IOptionsSnapshot<StatusCodesOptions> statusCodeOptions,
            IRouteProvider routeProvider,
            ILogger<ErrorHandlingMiddleware> logger)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error generating mocked response");

                mockacoContext.Errors.Add(new Error("Error generating mocked response", ex));
            }
            finally
            {
                if (mockacoContext.Errors.Any() && !httpContext.Response.HasStarted)
                {
                    httpContext.Response.StatusCode = (int)statusCodeOptions.Value.Error;
                    httpContext.Response.ContentType = HttpContentTypes.ApplicationJson;

                    IncludeRouteProviderErrors(mockacoContext, routeProvider);

                    await httpContext.Response.WriteAsync(mockacoContext.Errors.ToJson());
                }
            }
        }

        private static void IncludeRouteProviderErrors(IMockacoContext mockacoContext, IRouteProvider routeProvider)
        {
            mockacoContext.Errors
                .AddRange(routeProvider.GetErrors()
                    .Select(_ => new Error($"{_.TemplateName} - {_.ErrorMessage}")));
        }
    }
}
