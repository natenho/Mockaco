using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Mockaco.Templating.Generating.Source
{
    public class SourceContentProviderComposite : ISourceContentProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDictionary<Predicate<Uri>, Type> _providersRegistry = new Dictionary<Predicate<Uri>, Type>();

        public SourceContentProviderComposite(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            RegisterDefaultProviders();
        }

        private void RegisterDefaultProviders()
        {
            Register(uri => uri.Scheme.Equals(Uri.UriSchemeFile) || Path.IsPathFullyQualified(uri.OriginalString), typeof(LocalFileContentProvider));
            Register(uri => uri.Scheme.Equals(Uri.UriSchemeHttp) || uri.Scheme.Equals(Uri.UriSchemeHttps), typeof(HttpSourceContentProvider));
        }

        private ISourceContentProvider Create(Uri sourceUri)
        {
            foreach (var registryItem in _providersRegistry)
            {
                var canHandle = registryItem.Key;
                var providerType = registryItem.Value;

                if (canHandle(sourceUri))
                {
                    return (ISourceContentProvider)Activator.CreateInstance(registryItem.Value);
                }
            }

            throw new NotSupportedException("Specified URI is not supported");
        }

        public void Register(Predicate<Uri> canHandle, Type type)
        {
            _providersRegistry.Add(canHandle, type);
        }

        public Task<Stream> GetStreamAsync(Uri sourceUri, CancellationToken cancellationToken)
        {
            var specificProvider = Create(sourceUri);
            return specificProvider.GetStreamAsync(sourceUri, cancellationToken);
        }
    }
}
