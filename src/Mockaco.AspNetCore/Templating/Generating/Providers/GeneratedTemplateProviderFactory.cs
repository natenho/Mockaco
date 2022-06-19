using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Mockaco.Templating.Generating.Providers
{
    internal class GeneratedTemplateProviderFactory : IGeneratedTemplateProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly IDictionary<string, Type> _providersRegistry = new Dictionary<string, Type>
        {
            {"openapi", typeof(OpenApiTemplateProvider)}
        };

        public GeneratedTemplateProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        
        public IGeneratedTemplateProvider Create(string providerType)
        {
            if (_providersRegistry.ContainsKey(providerType))
            {
                return (IGeneratedTemplateProvider)_serviceProvider.GetRequiredService(_providersRegistry[providerType]);
            }

            throw new NotSupportedException($"Provider {providerType} is not supported.");
        }
    }
}