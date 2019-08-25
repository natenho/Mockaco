using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mockaco
{
    public partial class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache()
                .AddHttpClient()
                .AddOptions()

                .Configure<MockacoOptions>(_configuration.GetSection("Mockaco"))

                .AddScoped<IMockacoContext, MockacoContext>()
                .AddScoped<IScriptContext, ScriptContext>()

                .AddSingleton<IScriptRunnerFactory, ScriptRunnerFactory>()

                .AddSingleton<IFakerFactory, LocalizedFakerFactory>()
                .AddSingleton<IMockProvider, MockProvider>()
                .AddSingleton<ITemplateProvider, TemplateFileProvider>()

                .AddTransient<IRequestMatcher, RequestMethodMatcher>()
                .AddTransient<IRequestMatcher, RequestRouteMatcher>()

                .AddTransient<IRequestBodyFactory, RequestBodyFactory>()

                .AddTransient<IRequestBodyStrategy, FormRequestBodyStrategy>()
                .AddTransient<IRequestBodyStrategy, JsonRequestBodyStrategy>()
                .AddTransient<IRequestBodyStrategy, XmlRequestBodyStrategy>()

                .AddTransient<IResponseBodyFactory, ResponseBodyFactory>()

                .AddTransient<IResponseBodyStrategy, BinaryResponseBodyStrategy>()
                .AddTransient<IResponseBodyStrategy, JsonResponseBodyStrategy>()
                .AddTransient<IResponseBodyStrategy, XmlResponseBodyStrategy>()
                .AddTransient<IResponseBodyStrategy, DefaultResponseBodyStrategy>()
                
                .AddTransient<ITemplateTransformer, TemplateTransformer>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<ErrorHandlingMiddleware>()
                .UseMiddleware<ResponseDelayMiddleware>()
                .UseMiddleware<RequestMatchingMiddleware>()
                .UseMiddleware<ResponseMockingMiddleware>()
                .UseMiddleware<CallbackMiddleware>();
        }
    }
}