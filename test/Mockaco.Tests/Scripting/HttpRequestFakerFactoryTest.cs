using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq;
using Xunit;

namespace Mockaco.Tests.Scripting
{
    public class HttpRequestFakerFactoryTest
    {
        private HttpRequestFakerFactory _httpRequestFakerFactory;

        public HttpRequestFakerFactoryTest()
        {
            _httpRequestFakerFactory = new HttpRequestFakerFactory(Mock.Of<ILogger<HttpRequestFakerFactory>>());
        }

        [Fact]
        public void Gets_Localized_Faker_Based_On_Http_Accept_Language_Header()
        {            
            var faker = _httpRequestFakerFactory.GetFaker(new[] { "pt-BR" });

            faker.Locale.Should().Be("pt_BR");
        }

        [Fact]
        public void Gets_Default_Faker_When_No_Accept_Language_Header_Is_Present()
        { 
            var faker = _httpRequestFakerFactory.GetFaker(Enumerable.Empty<string>());

            faker.Locale.Should().Be("en");
        }
    }
}
