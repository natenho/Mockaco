using System;
using System.IO;
using System.Threading.Tasks;

namespace Mockaco.Generation.Readers
{
    public class LocalFileContentProvider : ISourceContentProvider
    {
        public Task<Stream> GetStreamAsync(Uri sourceUri)
        {
            return Task.FromResult((Stream)File.OpenRead(sourceUri.LocalPath));
        }
    }
}
