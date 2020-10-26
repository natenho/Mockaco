using Mockaco.Generation.Readers;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Mockaco.Tests.Generation.Readers
{
    public class LocalFileContentProviderTest : IDisposable
    {
        private readonly Uri _testFileUri;
        private readonly string _testFileContent = "Lore ipsum";
        public LocalFileContentProviderTest()
        {
            _testFileUri = new Uri(Path.GetFullPath($"{Guid.NewGuid():N}.txt"));
            File.WriteAllText(_testFileUri.LocalPath, _testFileContent);
        }

        [Fact]
        public async Task Should_Provide_Content_Stream_Without_Errors()
        {
            var provider = new LocalFileContentProvider();
            using var stream = await provider.GetStreamAsync(_testFileUri);
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();

            Assert.Equal(content, _testFileContent);
        }

        public void Dispose()
        {
            if (File.Exists(_testFileUri.LocalPath))
            {
                File.Delete(_testFileUri.LocalPath);
            }
        }
    }
}
