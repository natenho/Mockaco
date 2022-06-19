using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mockaco.Templating.Generating.Providers
{
    public interface IGeneratedTemplateProvider
    {
        Task<IEnumerable<GeneratedTemplate>> GetTemplatesAsync(Stream sourceStream, CancellationToken cancellationToken = default);
    }
}