using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mockaco.Processors;
using Mono.TextTemplating;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

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

            services.AddScoped<MockacoContext>();

            services.AddSingleton<ITemplateRepository, TemplateRepository>();
            services.AddSingleton<IScriptRunnerFactory, ScriptRunnerFactory>();

            services.AddTransient<ITemplateProcessor, TemplateProcessor>();
            services.AddTransient<ITemplateTransformer, TemplateTransformer>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<DelayMiddleware>();

            app.UseRouter(ConfigureRoute);
        }

        private static async void ConfigureRoute(IRouteBuilder routeBuilder)
        {
            var templateRepository = routeBuilder.ServiceProvider.GetService<ITemplateRepository>();

            foreach (var templateFile in templateRepository.GetAll())
            {
                var template = await ProcessTemplate(routeBuilder, templateFile);

                if (template == null)
                {
                    continue;
                }

                var httpMethod = template.Request.Method?.ToString() ?? HttpMethod.Get.ToString();
                var route = template.Request.Route ?? string.Empty;

                routeBuilder.MapVerb(httpMethod, route, RequestHandler);
            }
        }

        private static Task RequestHandler(HttpContext httpContext)
        {
            var processor = httpContext.RequestServices.GetRequiredService<ITemplateProcessor>();

            return processor.ProcessResponse(httpContext);
        }

        private static async Task<Template> ProcessTemplate(IRouteBuilder routeBuilder, TemplateFile templateFile)
        {
            Template template = null;

            var logger = routeBuilder.ServiceProvider.GetService<ILogger<Startup>>();

            try
            {
                var templateTransformer = routeBuilder.ServiceProvider.GetService<ITemplateTransformer>();
                var scriptContext = new ScriptContext();
                var parsedTemplate = await templateTransformer.Transform(templateFile.Content, scriptContext);

                template = JsonConvert.DeserializeObject<Template>(parsedTemplate);
            }
            catch (JsonReaderException ex)
            {
                logger.LogWarning("Skipping {0}: Generated JSON is invalid - {1}", templateFile.FileName, ex.Message);
            }
            catch (ParserException ex)
            {
                logger.LogWarning("Skipping {0}: Script parser error - {1} {2} ", templateFile.FileName, ex.Message, ex.Location);
            }
            catch (Exception ex)
            {
                logger.LogWarning("Skipping {0}: {1}", templateFile.FileName, ex.Message);
            }

            return template;
        }
    }
}