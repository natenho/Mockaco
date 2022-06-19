using System;
using System.Diagnostics;

namespace Mockaco
{
    [DebuggerDisplay("{Name}")]
    internal class RawTemplate : IRawTemplate
    {
        public string Name { get; }

        public string Content { get; }

        public string Hash { get; }

        public RawTemplate(string name, string content)
        {
            Name = name;
            Content = content;
            Hash = content.ToMD5Hash();
        }
    }
}