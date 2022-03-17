using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Mockaco
{
    using Settings;

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

            logger.LogInformation("{assemblyName} v{assemblyVersion} [github.com/natenho/Mockaco]\n\n{logo}", assemblyName, version, _logo);
            
            app
                .UseRouting()
                .UseCors()
                .UseMockaco();
        }
    }
}