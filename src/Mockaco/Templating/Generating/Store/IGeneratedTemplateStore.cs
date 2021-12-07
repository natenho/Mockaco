using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mockaco.Templating.Generating.Store
{
    public interface IGeneratedTemplateStore
    {
        Task SaveAsync(GeneratedTemplate template, CancellationToken cancellationToken = default);
        
        Task SaveAsync(IEnumerable<GeneratedTemplate> templates, CancellationToken cancellationToken = default);
    }
}