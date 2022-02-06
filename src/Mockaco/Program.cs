using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Mockaco
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            _ = Task.Factory.StartNew(() =>
            {
                using var scope = host.Services.CreateScope();
                var mockProvider = scope.ServiceProvider.GetService<IMockProvider>();

                mockProvider.BuildCache();
            });

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureAppConfiguration((_, configuration) =>
                    {
                        var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        configuration.SetBasePath(Path.Combine(assemblyLocation, "Settings"));
                        
                        var switchMappings = new Dictionary<string, string>() {
                            {"--path", "Mockaco:TemplateFileProvider:Path" },
                            {"--logs", "Serilog:WriteTo:0:Args:path" }                            
                        };

                        configuration.AddCommandLine(args, switchMappings);
                    })                    
                    .UseSerilog((context, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(context.Configuration))
                    .UseStartup<Startup>();
                });
    }
}