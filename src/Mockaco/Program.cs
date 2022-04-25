using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Mockaco
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var commandLine = CreateCommandLineBuilder(args, host)
                .UseDefaults()
                .Build();

            if (commandLine.IsUsingCommand(args))
            {
                await commandLine.InvokeAsync(args);
                return;
            }

            using (var scope = host.Services.CreateScope())
            {
                var mockProvider = scope.ServiceProvider.GetService<IMockProvider>();

                await mockProvider.WarmUp();
            }

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

        private static CommandLineBuilder CreateCommandLineBuilder(string[] args, IHost host)
        {
            var rootCommand = new RootCommand();

            foreach (var cmd in host.Services.GetServices<Command>())
            {
                rootCommand.AddCommand(cmd);
            }

            return new CommandLineBuilder(rootCommand);
        }
    }
}