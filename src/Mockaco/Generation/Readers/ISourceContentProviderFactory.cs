using System;

namespace Mockaco.Generation.Readers
{
    public interface ISourceContentProviderFactory
    {
        ISourceContentProvider Create(Uri sourceUri);
    }
}