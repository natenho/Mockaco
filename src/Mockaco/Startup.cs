using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace Mockaco
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;            
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();

            services.AddSingleton<ITemplateRepository, TemplateRepository>();
            services.AddSingleton<IScriptRunnerFactory, ScriptRunnerFactory>();

            services.AddTransient<ITemplateProcessor, TemplateProcessor>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouter(
                r =>
                {
                    var templateRepository = app.ApplicationServices.GetService<ITemplateRepository>();

                    foreach (var template in templateRepository.GetAll())
                    {
                        r.MapVerb(
                            template.Request.Method?.ToString() ?? HttpMethod.Get.ToString(),
                            template.Request.Route ?? string.Empty,
                            httpContext =>
                            {
                                var processor = app.ApplicationServices.GetRequiredService<ITemplateProcessor>();

                                return processor.ProcessResponse(httpContext);
                            });
                    }
                });            
        }
    }
}