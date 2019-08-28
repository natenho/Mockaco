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
        private readonly BinaryResponseBodyStrategy _strategy;

        public BinaryResponseBodyStrategyTest()
        {
            var httpClientFactory = Moq.Mock.Of<IHttpClientFactory>();

            _strategy = new BinaryResponseBodyStrategy(httpClientFactory);
        }

        [Fact]
        public void Can_Handle_Templates_With_Response_File_Attribute()
        {
            var responseTemplate = new ResponseTemplate { File = "somefile.bin" };

            _strategy.CanHandle(responseTemplate)
                .Should()
                .BeTrue();
        }

        [Fact]
        public void Can_Handle_Templates_With_Response_Body_Attribute_And_Application_Octet_Stream_Content_Type()
        {
            var responseTemplate = new ResponseTemplate
            {
                Headers = new StringDictionary { { HttpHeaders.ContentType, HttpContentTypes.ApplicationOctetStream } },
                Body = "randomdata"
            };

            _strategy.CanHandle(responseTemplate)
                .Should()
                .BeTrue();
        }

        [Fact]
        public void Can_Not_Handle_Templates_With_Response_Body_Attribute_And_No_Content_Type()
        {
            var responseTemplate = new ResponseTemplate { Body = "randomdata" };

            _strategy.CanHandle(responseTemplate)
                .Should()
                .BeFalse();
        }

        [Fact]
        public async Task Returns_Response_For_Binary_File()
        {
            const string responseFilePath = "Templating/Response/mockaco.jpg";

            var responseTemplate = new ResponseTemplate { File = responseFilePath };

            var bodyBytes = await _strategy.GetResponseBodyBytesFromTemplate(responseTemplate);

            var fileBytes = await File.ReadAllBytesAsync(responseFilePath);

            bodyBytes.Should()
                .BeEquivalentTo(fileBytes);
        }

        [Fact]
        public async Task Throws_When_Both_Body_And_File_Attributes_Are_Set()
        {
            var responseTemplate = new ResponseTemplate { File = "somefile.bin", Body = "randomdata" };

            Func<Task> action = async () => await _strategy.GetResponseBodyBytesFromTemplate(responseTemplate);

            await action.Should()
                .ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task Returns_Null_For_Null_Body()
        {
            var nullBodyResponseTemplate = new ResponseTemplate { Body = null };

            var response = await _strategy.GetResponseBodyBytesFromTemplate(nullBodyResponseTemplate);

            response.Should()
                .BeNull();
        }
    }
}