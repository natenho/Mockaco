using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Mockaco.Generation.Readers
{
    public class SourceContentProviderFactory : ISourceContentProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDictionary<Predicate<Uri>, Type> _providersRegistry = new Dictionary<Predicate<Uri>, Type>();

        public SourceContentProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            RegisterDefaultProviders();
        }

        private void RegisterDefaultProviders()
        {
            Register(uri => uri.Scheme.Equals(Uri.UriSchemeFile), typeof(LocalFileContentProvider));
        }

        public ISourceContentProvider Create(Uri sourceUri)
        {
            foreach (var registryItem in _providersRegistry)
            {
                var canHandle = registryItem.Key;
                var providerType = registryItem.Value;

                if (canHandle(sourceUri))
                {
                    return (ISourceContentProvider)_serviceProvider.GetRequiredService(providerType);
                }
            }

            throw new NotSupportedException("Specified URI is not suported");
        }

        public void Register(Predicate<Uri> canHandle, Type type)
        {
            _providersRegistry.Add(canHandle, type);
        }
    }
}
