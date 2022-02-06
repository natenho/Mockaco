using System;

namespace Mockaco
{
    public interface IRawTemplate : IEquatable<IRawTemplate>
    {
        string Content { get; }

        string Name { get; }

        string Hash { get; }

        DateTime LastModified { get; }
    }
}