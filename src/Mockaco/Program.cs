using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mockaco.Routing;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
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
            .ConfigureAppConfiguration((_, configuration) => configuration.AddJsonFile(@"Settings/appsettings.json", optional: true, reloadOnChange: true))
            .UseSerilog((context, serilog) =>
                serilog.WriteTo.Console(theme: AnsiConsoleTheme.Literate)
                .ReadFrom.Configuration(context.Configuration))
            .UseStartup<Startup>();
    }
}