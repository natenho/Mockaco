using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mockaco.Routing;
using System.Threading.Tasks;

namespace Mockaco
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var webHost = CreateWebHostBuilder(args).Build();

            using (var scope = webHost.Services.CreateScope())
            {
                var routeProvider = scope.ServiceProvider.GetService<IRouteProvider>();

                await routeProvider.WarmUp();
            }

            await webHost.RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(delegate (WebHostBuilderContext hostingContext, IConfigurationBuilder config)
            {
                config.AddJsonFile(@"Settings\appsettings.json", optional: true, reloadOnChange: true);                
            })
            .UseStartup<Startup>();
    }
}