using System.Threading;
using System.Threading.Tasks;
using Mockaco.Templating.Generating.Providers;
using Mockaco.Templating.Generating.Source;
using Mockaco.Templating.Generating.Store;

namespace Mockaco.Templating.Generating
{
    public class TemplatesGenerator
    {
        private readonly ISourceContentProvider _sourceContentProvider;
        private readonly IGeneratedTemplateProviderFactory _providerFactory;
        private readonly IGeneratedTemplateStore _templateStore;

        public TemplatesGenerator(ISourceContentProvider sourceContentProvider, IGeneratedTemplateProviderFactory providerFactory, IGeneratedTemplateStore templateStore)
        {
            _sourceContentProvider = sourceContentProvider;
            _providerFactory = providerFactory;
            _templateStore = templateStore;
        }
        
        public async Task GenerateAsync(GeneratingOptions options, CancellationToken cancellationToken = default)
        {
            var sourceStream = await _sourceContentProvider.GetStreamAsync(options.SourceUri, cancellationToken);
            var templates = await _providerFactory.Create(options.Provider).GetTemplatesAsync(sourceStream, cancellationToken);
            await _templateStore.SaveAsync(templates, cancellationToken);
        }
    }
}