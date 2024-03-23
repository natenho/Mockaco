using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System;
using System.Linq;

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
                .AddCors()
                .AddMockaco(_configuration.GetSection("Mockaco"));
        }

        public void Configure(IApplicationBuilder app, ILogger<Startup> logger)
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            var version = GitVersionInformation.InformationalVersion;
            var isNoLogoPassed = Environment.GetCommandLineArgs().Contains("--no-logo");

            var logMessage = "{assemblyName} v{assemblyVersion} [github.com/natenho/Mockaco]";

            if (!isNoLogoPassed)
                logMessage += "\n\n{logo}";

            logger.LogInformation(logMessage, assemblyName, version, _logo);

            app
                .UseCors()
                .UseMockaco();
        }
    }
}