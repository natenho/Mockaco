using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Mockaco
{
    [DebuggerDisplay("{Name}")]
    public class RawTemplate : IRawTemplate
    {
        public string Name { get; }

        public string Content { get; }

        public string Hash { get; }

        public DateTime LastModified { get; }

        public RawTemplate(string name, string content, DateTime lastModified)
        {
            Name = name;
            Content = content;
            Hash = content.ToSHA1Hash();
            LastModified = lastModified;
        }

        public override string ToString()
        {
            return $"{Name} ({Hash})";
        }

        public bool Equals([AllowNull] IRawTemplate other)
        {
            return other.Hash == Hash;
        }
    }
}