using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mockaco.Middlewares;
using Mockaco.Processors;
using Mockaco.Routing;
using Mockaco.Templating;

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
            services
                .AddMemoryCache()
                .AddHttpClient()

                .AddScoped<IMockacoContext, MockacoContext>()
                .AddScoped<IScriptContext, ScriptContext>()

                .AddSingleton<IScriptRunnerFactory, ScriptRunnerFactory>()

                .AddSingleton<IFakerFactory, HttpRequestFakerFactory>()
                .AddSingleton<IRouteProvider, RouteProvider>()
                .AddSingleton<ITemplateProvider, TemplateFileProvider>()

                .AddTransient<IResponseBodyStrategyFactory, ResponseBodyStrategyFactory>()

                .AddTransient<IResponseBodyStrategy, JsonResponseBodyStrategy>()
                .AddTransient<IResponseBodyStrategy, XmlResponseBodyStrategy>()
                .AddTransient<IResponseBodyStrategy, DefaultResponseBodyStrategy>()

                .AddTransient<ITemplateResponseProcessor, TemplateResponseProcessor>()
                .AddTransient<ITemplateTransformer, TemplateTransformer>()

                .AddOptions()
                .Configure<StatusCodesOptions>(_configuration.GetSection("StatusCodes"));
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