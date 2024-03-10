using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit.Sdk;

namespace Mockaco.Tests
{
    public class TextFileDataAttribute : DataAttribute
    {
        private readonly string _filePath;

        public TextFileDataAttribute(string filePath)
        {
            _filePath = filePath;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            if (testMethod == null) { throw new ArgumentNullException(nameof(testMethod)); }

            var path = Path.IsPathRooted(_filePath)
                ? _filePath
                : Path.GetRelativePath(Directory.GetCurrentDirectory(), _filePath);

            if (!File.Exists(path))
            {
                throw new ArgumentException($"Could not find file at path: {path} (current directory is {Directory.GetCurrentDirectory()})");
            }

            var fileData = File.ReadAllText(_filePath);

            yield return new object[] { fileData };
        }
    }
}
