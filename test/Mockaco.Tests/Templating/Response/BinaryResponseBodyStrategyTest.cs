using FluentAssertions;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Mockaco.Tests.Templating.Response
{
    public class BinaryResponseBodyStrategyTest
    {
        private IHttpClientFactory _httpClientFactory;

        public BinaryResponseBodyStrategyTest()
        {
            _httpClientFactory = Moq.Mock.Of<IHttpClientFactory>();
        }

        [Fact]
        public void Can_Handle_Templates_With_Response_File_Attribute()
        {
            var strategy = new BinaryResponseBodyStrategy(_httpClientFactory);

            var responseTemplate = new ResponseTemplate
            {
                File = "somefile.bin"
            };

            strategy.CanHandle(responseTemplate)
                .Should().BeTrue();
        }

        [Fact]
        public void Can_Handle_Templates_With_Response_Body_Attribute_And_Application_Octet_Stream_Content_Type()
        {
            var strategy = new BinaryResponseBodyStrategy(_httpClientFactory);

            var responseTemplate = new ResponseTemplate
            {
                Headers = new StringDictionary { { HttpHeaders.ContentType, HttpContentTypes.ApplicationOctetStream } },
                Body = "randomdata"
            };

            strategy.CanHandle(responseTemplate)
                .Should().BeTrue();
        }

        [Fact]
        public void Can_Not_Handle_Templates_With_Response_Body_Attribute_And_No_Content_Type()
        {
            var strategy = new BinaryResponseBodyStrategy(_httpClientFactory);

            var responseTemplate = new ResponseTemplate
            {
                Body = "randomdata"
            };

            strategy.CanHandle(responseTemplate)
                .Should().BeFalse();
        }

        [Fact]
        public async Task Returns_Response_For_Binary_File()
        {
            var strategy = new BinaryResponseBodyStrategy(_httpClientFactory);

            var responseFilePath = "Templating/Response/mockaco.jpg";

            var responseTemplate = new ResponseTemplate
            {
                File = responseFilePath
            };

            var bodyBytes = await strategy.GetResponseBodyBytesFromTemplate(responseTemplate);

            var fileBytes = await File.ReadAllBytesAsync(responseFilePath);

            bodyBytes.Should()
                .BeEquivalentTo(fileBytes);
        }

        [Fact]
        public async Task Throws_When_Both_Body_And_File_Attributes_Are_Set()
        {
            var strategy = new BinaryResponseBodyStrategy(_httpClientFactory);

            var responseTemplate = new ResponseTemplate
            {
                File = "somefile.bin",
                Body = "randomdata"
            };

            Func<Task> action = async () => await strategy.GetResponseBodyBytesFromTemplate(responseTemplate);

            await action.Should()
                .ThrowAsync<InvalidOperationException>();                        
        }
    }
}
