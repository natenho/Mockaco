using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Mockaco;
using Mockaco.Chaos.Strategies;
using Mockaco.HealthChecks;
using Mockaco.Settings;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MockacoServiceCollection
    {
        public static IServiceCollection AddMockaco(this IServiceCollection services) =>
            services.AddMockaco(_ => { });

        public static IServiceCollection AddMockaco(this IServiceCollection services, Action<MockacoOptions> config) =>
            services
                .AddOptions<MockacoOptions>().Configure(config).Services
                .AddOptions<ChaosOptions>().Configure<IOptions<MockacoOptions>>((options, parent) => options = parent.Value.Chaos).Services
                .AddOptions<TemplateFileProviderOptions>()
                    .Configure<IOptions<MockacoOptions>>((options, parent) => options = parent.Value.TemplateFileProvider)
                    .Services
                .AddCommonServices();


        public static IServiceCollection AddMockaco(this IServiceCollection services, IConfiguration config) =>
            services
                .AddConfiguration(config)
                .AddCommonServices();

        private static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration config) =>
            services
                .AddOptions()
                .Configure<MockacoOptions>(config)
                .Configure<ChaosOptions>(config.GetSection("Chaos"))
                .Configure<TemplateFileProviderOptions>(config.GetSection("TemplateFileProvider"));

        private static IServiceCollection AddCommonServices(this IServiceCollection services)
        {
            services
                .AddMemoryCache()
                .AddHttpClient()
                .AddInternalServices()
                .AddChaosServices()
                .AddHostedService<MockProviderWarmUp>();

            services
                .AddSingleton<StartupHealthCheck>()
                .AddHealthChecks()
                    .AddCheck<StartupHealthCheck>("Startup", tags: new[] { "ready" });

            return services;
        }

        private static IServiceCollection AddInternalServices(this IServiceCollection services) =>
            services
                .AddSingleton<VerificationRouteValueTransformer>()
                .AddScoped<IMockacoContext, MockacoContext>()
                .AddScoped<IScriptContext, ScriptContext>()
                .AddTransient<IGlobalVariableStorage, ScriptContextGlobalVariableStorage>()

                .AddSingleton<IScriptRunnerFactory, ScriptRunnerFactory>()

                .AddSingleton<IFakerFactory, LocalizedFakerFactory>()
                .AddSingleton<IMockProvider, MockProvider>()
                .AddSingleton<ITemplateProvider, TemplateFileProvider>()

                .AddScoped<IRequestMatcher, RequestMethodMatcher>()
                .AddScoped<IRequestMatcher, RequestRouteMatcher>()
                .AddScoped<IRequestMatcher, RequestConditionMatcher>()

                .AddTransient<IRequestBodyFactory, RequestBodyFactory>()

                .AddTransient<IRequestBodyStrategy, JsonRequestBodyStrategy>()
                .AddTransient<IRequestBodyStrategy, XmlRequestBodyStrategy>()
                .AddTransient<IRequestBodyStrategy, FormRequestBodyStrategy>()

                .AddTransient<IResponseBodyFactory, ResponseBodyFactory>()

                .AddTransient<IResponseBodyStrategy, BinaryResponseBodyStrategy>()
                .AddTransient<IResponseBodyStrategy, JsonResponseBodyStrategy>()
                .AddTransient<IResponseBodyStrategy, XmlResponseBodyStrategy>()
                .AddTransient<IResponseBodyStrategy, DefaultResponseBodyStrategy>()

                .AddTransient<ITemplateTransformer, TemplateTransformer>()

                .AddTemplatesGenerating();
        
        private static IServiceCollection AddChaosServices(this IServiceCollection services) =>
            services
                .AddSingleton<IChaosStrategy, ChaosStrategyBehavior>()
                .AddSingleton<IChaosStrategy, ChaosStrategyException>()
                .AddSingleton<IChaosStrategy, ChaosStrategyLatency>()
                .AddSingleton<IChaosStrategy, ChaosStrategyResult>()
                .AddSingleton<IChaosStrategy, ChaosStrategyTimeout>();
    }
}
