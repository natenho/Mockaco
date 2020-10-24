using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mockaco.Commands;
using Serilog;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Mockaco
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder().Build();

            var app = new CommandLineApplication<Program>();
            app.Conventions
            .UseDefaultConventions()
            .UseConstructorInjection(host.Services);

            app.Name = Assembly.GetExecutingAssembly().GetName().Name.ToLower();
            app.FullName = $"{Assembly.GetExecutingAssembly().GetName().Name} {Assembly.GetExecutingAssembly().GetName().Version}";

            app.Command("run", config =>
            {
                config.Description = "Runs mock server";
                config.OnExecuteAsync(RunServer(host));
            });

            host.Services.GetRequiredService<GenerateCommand>().SelfRegister(app);

            app.OnExecuteAsync(RunServer(host));

            await app.ExecuteAsync(args);
        }

        private static Func<CancellationToken, Task> RunServer(IHost host)
        {
            return async (cancelationToken) =>
            {
                using (var scope = host.Services.CreateScope())
                {
                    var mockProvider = scope.ServiceProvider.GetService<IMockProvider>();

                    await mockProvider.WarmUp();
                }

                await host.RunAsync(cancelationToken);
            };
        }

        public static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureAppConfiguration((_, configuration) => configuration.AddJsonFile("Settings/appsettings.json", optional: true, reloadOnChange: true))
                    .UseSerilog((context, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(context.Configuration))
                    .UseStartup<Startup>();
                });
    }
}