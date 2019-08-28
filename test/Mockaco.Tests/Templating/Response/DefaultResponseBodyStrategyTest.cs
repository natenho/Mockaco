using FluentAssertions;
using Xunit;

namespace Mockaco.Tests.Templating
{
    public class DefaultResponseBodyStrategyTest
    {
        private readonly DefaultResponseBodyStrategy _strategy;
        private readonly ResponseTemplate _defaulResponseTemplate;
        private readonly ResponseTemplate _responseTemplateWithContentType;

        public DefaultResponseBodyStrategyTest()
        {
            _strategy = new DefaultResponseBodyStrategy();

            _defaulResponseTemplate = new ResponseTemplate();

            _responseTemplateWithContentType = new ResponseTemplate();
            _responseTemplateWithContentType.Headers.Add(HttpHeaders.ContentType, HttpContentTypes.ApplicationJson);
        }

        [Fact]
        public void Can_Handle_Response_Template_With_Default_Properties()
        {
            _strategy.CanHandle(_defaulResponseTemplate)
                .Should()
                .BeTrue();
        }

        [Fact]
        public void Can_Handle_Response_Template_With_Content_Type()
        {
            _strategy.CanHandle(_responseTemplateWithContentType)
                .Should()
                .BeTrue();
        }

        [Fact]
        public void Returns_Response_For_Any_Response_Template_By_Default()
        {
            var response = _strategy.GetResponseBodyStringFromTemplate(_defaulResponseTemplate);

            response.Should()
                .BeNull();
        }

        [Fact]
        public void Returns_Null_For_Null_Body()
        {
            var nullBodyResponseTemplate = new ResponseTemplate { Body = null };

            var response = _strategy.GetResponseBodyStringFromTemplate(nullBodyResponseTemplate);

            response.Should()
                .BeNull();
        }
    }
}