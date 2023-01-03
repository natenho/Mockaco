﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mockaco;
using Mockaco.AdminApi;
using Mockaco.Verifyer;

namespace Microsoft.AspNetCore.Builder
{
    public static class MockacoApplicationBuilder
    {
        public static IApplicationBuilder UseMockaco(this IApplicationBuilder app, Action<IApplicationBuilder> configure)
        {
            app.UseRouting();
            var options = app.ApplicationServices.GetRequiredService<IOptions<MockacoOptions>>().Value;
            app.UseEndpoints(endpoints => endpoints.Map($"/{options.VerificationEndpointPrefix}/{options.VerificationEndpointName}", VerifyerExtensions.Verify));

            app.UseEndpoints(endpoints => endpoints.Map($"/{options.AdminApiEndpointPrefix}/{options.AdminApiEndpointName}/{{action}}", AdminApiExtensions.Handler));

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
