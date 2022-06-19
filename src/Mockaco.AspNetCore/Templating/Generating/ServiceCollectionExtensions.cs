using Mockaco.Templating.Generating;
using Mockaco.Templating.Generating.Cli;
using Mockaco.Templating.Generating.Providers;
using Mockaco.Templating.Generating.Source;
using Mockaco.Templating.Generating.Store;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Microsoft.Extensions.DependencyInjection
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTemplatesGenerating(this IServiceCollection services)
        {
            services
                .AddTransient<IGeneratedTemplateStore, GeneratedTemplateStore>()
                .AddTransient<ISourceContentProvider, SourceContentProviderComposite>()
                .AddTransient<IGeneratedTemplateProviderFactory, GeneratedTemplateProviderFactory>()
                .AddTransient<OpenApiTemplateProvider>()
                .AddTransient<TemplatesGenerator>();
            
            services.AddOptions<TemplateStoreOptions>()
                    .Configure(options =>
                    {
                        options.RootDir = "Mocks";
                    })
                    .BindConfiguration("Mockaco:TemplateFileProvider:Path");

            services.AddTransient<GeneratorRunner>();
            services.AddTransient(provider =>
            {
                var cmd = new Command("generate")
                {
                    new Argument<string>("source"),
                    new Option<string>(new[] { "--provider"})
                    {
                        IsRequired = true
                    },
                    new Option<string>("--path")
                };

                cmd.Handler = CommandHandler.Create<GeneratingOptions, IConsole, CancellationToken>(
                    provider.GetRequiredService<GeneratorRunner>().ExecuteAsync);
                

                return cmd;
            });
            
            return services;
        }
    }
}