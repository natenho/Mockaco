using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mockaco;
using Mockaco.Verifyer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Microsoft.AspNetCore.Builder
{
    public static class MockacoApplicationBuilder
    {
        public static IApplicationBuilder UseMockaco(this IApplicationBuilder app, Action<IApplicationBuilder> configure)
        {
            app.UseRouting();

            var options = app.ApplicationServices.GetRequiredService<IOptions<MockacoOptions>>().Value;

            app.UseEndpoints(endpoints =>
            {
                endpoints.Map($"/{options.VerificationEndpointPrefix ?? options.MockacoEndpoint}/{options.VerificationEndpointName}", VerifyerExtensions.Verify);

                endpoints.MapHealthChecks($"/{options.MockacoEndpoint}/ready", new HealthCheckOptions
                {
                    Predicate = healthCheck => healthCheck.Tags.Contains("ready")
                });

                endpoints.MapHealthChecks($"/{options.MockacoEndpoint}/health", new HealthCheckOptions
                {
                    Predicate = _ => false
                });
            });

            app.UseMiddleware<ErrorHandlingMiddleware>();
            configure(app);
            app
                .UseMiddleware<RequestMatchingMiddleware>()
                .UseMiddleware<ResponseDelayMiddleware>()
                .UseMiddleware<ResponseMockingMiddleware>()
                .UseMiddleware<CallbackMiddleware>();

            return app;
        }

        public static IApplicationBuilder UseMockaco(this IApplicationBuilder app) =>
            app.UseMockaco(_ => { });
    }
}
