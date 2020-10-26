using System;
using System.IO;
using System.Threading.Tasks;

namespace Mockaco.Generation.Readers
{
    public interface ISourceContentProvider
    {
        Task<Stream> GetStreamAsync(Uri sourceUri);
    }
}
