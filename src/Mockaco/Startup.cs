using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mockaco.Commands;
using System.Reflection;

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
                .Configure<TemplateFileProviderOptions>(_configuration.GetSection("Mockaco:TemplateFileProvider"))

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

                .AddTransient<IRequestBodyStrategy, FormRequestBodyStrategy>()
                .AddTransient<IRequestBodyStrategy, JsonRequestBodyStrategy>()
                .AddTransient<IRequestBodyStrategy, XmlRequestBodyStrategy>()

                .AddTransient<IResponseBodyFactory, ResponseBodyFactory>()

                .AddTransient<IResponseBodyStrategy, BinaryResponseBodyStrategy>()
                .AddTransient<IResponseBodyStrategy, JsonResponseBodyStrategy>()
                .AddTransient<IResponseBodyStrategy, XmlResponseBodyStrategy>()
                .AddTransient<IResponseBodyStrategy, DefaultResponseBodyStrategy>()

                .AddTransient<ITemplateTransformer, TemplateTransformer>()

            //Commands

                .AddScoped<GenerateCommand>();
        }

        public void Configure(IApplicationBuilder app, ILogger<Startup> logger)
        {
            AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
            logger.LogInformation($"{assemblyName.Name} v{assemblyName.Version} by Renato Lima [github.com/natenho]\n\n{_banner}");

            app.UseMiddleware<ErrorHandlingMiddleware>()
                .UseMiddleware<ResponseDelayMiddleware>()
                .UseMiddleware<RequestMatchingMiddleware>()
                .UseMiddleware<ResponseMockingMiddleware>()
                .UseMiddleware<CallbackMiddleware>();
        }
    }
}