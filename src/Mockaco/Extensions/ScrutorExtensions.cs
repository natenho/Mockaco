using Microsoft.Extensions.Logging;
using Scrutor;
using System.IO;
using System.Linq;
using System.Reflection;

public static class ScrutorExtensions
{
    public static IImplementationTypeSelector FromPluginAssemblies(this ITypeSourceSelector typeSourceSelector, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(nameof(ScrutorExtensions));

        var path = Path.Combine(Path.GetDirectoryName(typeof(ScrutorExtensions).Assembly.Location), "Plugins");

        logger.LogInformation("Loading plugins from {pluginsPath}", path);
                
        var pluginAssemblies = Directory.GetFiles(path, "*.dll")
            .Select(file =>
                {
                    logger.LogInformation("Loading plugin {pluginFileName}", file);
                    
                    return Assembly.LoadFrom(file);
                }
            )
            .ToArray();

        logger.LogInformation("{pluginCount} plugin(s) loaded", pluginAssemblies.Length);

        return typeSourceSelector.FromAssemblies(pluginAssemblies);
    }

    public static IServiceTypeSelector AddClassesAssignableTo<T>(this IImplementationTypeSelector implementationTypeSelector)
    {
        return implementationTypeSelector.AddClasses(classes => classes.AssignableTo<T>());
    }
}