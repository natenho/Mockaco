using Mockaco.Generation.Readers;
using System.Threading.Tasks;

namespace Mockaco.Generation
{
    public class TemplateGenerator : ITemplateGenerator
    {
        private readonly ISourceContentProviderFactory _sourceContentProviderFactory;

        public TemplateGenerator(ISourceContentProviderFactory sourceContentProviderFactory)
        {
            _sourceContentProviderFactory = sourceContentProviderFactory;
        }

        public async Task GenerateTemplateAsync(TemplateGenerationContext context)
        {
            var provider = _sourceContentProviderFactory.Create(context.SourceUri);
            using(var contentStream = await provider.GetStreamAsync(context.SourceUri))
            {
                
            }
        }
    }
}
