using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mockaco.Templating.Generating.Source
{
    public interface ISourceContentProvider
    {
        Task<Stream> GetStreamAsync(Uri sourceUri, CancellationToken cancellationToken);
    }
}
