using FluentAssertions;
using Microsoft.Extensions.Logging;
using System.Linq;
using Xunit;

namespace Mockaco.Tests.Scripting
{
    public class LocalizedFakerFactoryTest
    {
        private LocalizedFakerFactory _localizedFakerFactory;

        public LocalizedFakerFactoryTest()
        {
            _localizedFakerFactory = new LocalizedFakerFactory(Moq.Mock.Of<ILogger<LocalizedFakerFactory>>());
        }

        [Fact]
        public void Gets_Localized_Faker_Based_On_Http_Accept_Language_Header()
        {            
            var faker = _localizedFakerFactory.GetFaker(new[] { "pt-BR" });

            faker.Locale.Should().Be("pt_BR");
        }

        [Fact]
        public void Gets_Default_Faker_When_No_Accept_Language_Header_Is_Present()
        { 
            var faker = _localizedFakerFactory.GetFaker(Enumerable.Empty<string>());

            faker.Locale.Should().Be("en");
        }
    }
}
