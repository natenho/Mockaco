using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mockore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();

            services.AddSingleton<ITemplateRepository, TemplateRepository>();
            services.AddTransient<ITemplateProcessor, TemplateProcessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouter(
                r =>
                {
                    var templateRepository = app.ApplicationServices.GetService<ITemplateRepository>();

                    foreach (var template in templateRepository.GetAll())
                    {
                        r.MapVerb(
                            template.Request.Method.ToString(),
                            template.Request.Route,
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