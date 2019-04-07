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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddHttpClient();

            services.AddScoped<IMockacoContext, MockacoContext>();
            services.AddScoped<IScriptContext, ScriptContext>();

            services.AddSingleton<IScriptRunnerFactory, ScriptRunnerFactory>();
            
            services.AddSingleton<IRouteProvider, RouteProvider>();
            services.AddSingleton<ITemplateProvider, TemplateFileProvider>();
            
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