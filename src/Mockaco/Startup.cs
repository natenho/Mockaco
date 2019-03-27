using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mockaco.Processors;
using Mono.TextTemplating;
using Newtonsoft.Json;
using System;
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

            services.AddScoped<MockacoContext>();

            services.AddSingleton<ITemplateRepository, TemplateRepository>();
            services.AddSingleton<IScriptRunnerFactory, ScriptRunnerFactory>();

            services.AddTransient<ITemplateProcessor, TemplateProcessor>();
            services.AddTransient<ITemplateTransformer, TemplateTransformer>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<DelayMiddleware>();

            app.UseRouter(
                async routeBuilder =>
                {
                    var templateRepository = routeBuilder.ServiceProvider.GetService<ITemplateRepository>();
                    var templateTransformer = routeBuilder.ServiceProvider.GetService<ITemplateTransformer>();
                    var logger = routeBuilder.ServiceProvider.GetService<ILogger<Startup>>();

                    var scriptContext = new ScriptContext();
                    
                    foreach (var templateFile in templateRepository.GetAll())
                    {
                        Template template;
                        try
                        {
                            var parsedTemplate = await templateTransformer.Transform(templateFile.Content, scriptContext);
                            template = JsonConvert.DeserializeObject<Template>(parsedTemplate);
                        }
                        catch (JsonReaderException ex)
                        {
                            logger.LogWarning("Skipping {0}: Generated JSON is invalid - {1}", templateFile.FileName, ex.Message);
                            continue;
                        }
                        catch (ParserException ex)
                        {
                            logger.LogWarning("Skipping {0}: Script parser error - {1} {2} ", templateFile.FileName, ex.Message, ex.Location);
                            continue;
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning("Skipping {0}: {1}", templateFile.FileName, ex.Message);
                            continue;
                        }
                        
                        routeBuilder.MapVerb(
                            template.Request.Method?.ToString() ?? HttpMethod.Get.ToString(), // TODO Find a better way to resolve template.Request here
                            template.Request.Route ?? string.Empty, // TODO Find a better way to resolve template.Request here
                            httpContext =>
                            {
                                var processor = httpContext.RequestServices.GetRequiredService<ITemplateProcessor>();
                                return processor.ProcessResponse(httpContext);
                            });
                    }
                });
        }
    }
}