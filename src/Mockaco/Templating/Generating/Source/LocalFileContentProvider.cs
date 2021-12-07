using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mockaco.Templating.Generating.Source
{
    public class LocalFileContentProvider : ISourceContentProvider
    {
        public Task<Stream> GetStreamAsync(Uri sourceUri, CancellationToken cancellationToken)
        {
            return Task.FromResult((Stream)File.OpenRead(sourceUri.LocalPath));
        }
    }
}
