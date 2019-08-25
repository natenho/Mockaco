using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            IOptionsSnapshot<MockacoOptions> statusCodeOptions,
            IMockProvider mockProvider,
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
                    httpContext.Response.StatusCode = (int)statusCodeOptions.Value.ErrorHttpStatusCode;
                    httpContext.Response.ContentType = HttpContentTypes.ApplicationJson;

                    IncludeMockProviderErrors(mockacoContext, mockProvider);

                    await httpContext.Response.WriteAsync(mockacoContext.Errors.ToJson());
                }
            }
        }

        private static void IncludeMockProviderErrors(IMockacoContext mockacoContext, IMockProvider mockProvider)
        {
            mockacoContext.Errors
                .AddRange(mockProvider.GetErrors()
                    .Select(_ => new Error($"{_.TemplateName} - {_.ErrorMessage}")));
        }
    }
}
