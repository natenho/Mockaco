using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mockaco.Processors;

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
            services.AddRouting();
            services.AddHttpClient();

            services.AddScoped<MockacoContext>();

            services.AddSingleton<ITemplateRepository, CachingTemplateFileRepository>();
            services.AddSingleton<TemplateFileRepository>();

            services.AddSingleton<IScriptRunnerFactory, ScriptRunnerFactory>();

            services.AddTransient<ITemplateProcessor, TemplateProcessor>();
            services.AddTransient<ITemplateTransformer, TemplateTransformer>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<DelayMiddleware>();

            var router = ActivatorUtilities.CreateInstance<ReloadableRouter>(app.ApplicationServices, app);
            app.UseRouter(router);
        }
    }
}