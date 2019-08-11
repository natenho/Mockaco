using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Mockaco.Middlewares;
using Mockaco.Processors;
using Mockaco.Routing;
using Mockaco.Templating;

namespace Mockaco
{
    public partial class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddHttpClient();

            services.AddScoped<IMockacoContext, MockacoContext>();            
            services.AddScoped<IScriptContext, ScriptContext>();            

            services.AddSingleton<IScriptRunnerFactory, ScriptRunnerFactory>();

            services.AddSingleton<IFakerFactory, HttpRequestFakerFactory>();
            services.AddSingleton<IRouteProvider, RouteProvider>();
            services.AddSingleton<ITemplateProvider, TemplateFileProvider>();

            services.AddTransient<IResponseBodyStrategyFactory, ResponseBodyStrategyFactory>();

            services.AddTransient<IResponseBodyStrategy, JsonResponseBodyStrategy>();
            services.AddTransient<IResponseBodyStrategy, XmlResponseBodyStrategy>();
            services.AddTransient<IResponseBodyStrategy, DefaultResponseBodyStrategy>();

            services.AddTransient<ITemplateResponseProcessor, TemplateResponseProcessor>();
            services.AddTransient<ITemplateTransformer, TemplateTransformer>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<ResponseDelayMiddleware>();
            app.UseMiddleware<RequestMatchingMiddleware>();
            app.UseMiddleware<ResponseMockingMiddleware>();
            app.UseMiddleware<CallbackMiddleware>();
        }
    }
}