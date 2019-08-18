using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;

namespace Mockaco.Tests.Templating
{
    public class JsonResponseBodyStrategyTest
    {
        private const string UnindentedValidJson = "{\"property1\":\"property1Value\"}";
        private const string IndentedValidJson = "{\r\n  \"property1\": \"property1Value\"\r\n}";

        private JsonResponseBodyStrategy _strategy;
        private ResponseTemplate _validJsonResponseTemplate;

        public JsonResponseBodyStrategyTest()
        {
            _strategy = new JsonResponseBodyStrategy();

            _validJsonResponseTemplate = new ResponseTemplate
            {
                Body = JToken.Parse(UnindentedValidJson)
            };
        }

        [Fact]
        public void Can_Handle_Valid_Json_Response_Body_By_Default()
        {
            _strategy.CanHandle(_validJsonResponseTemplate)
                .Should().BeTrue();
        }

        [Fact]
        public void Can_Handle_Valid_Json_Response_Body_With_Application_Json_Content_Type_Header()
        {
            _validJsonResponseTemplate.Headers.Add(HttpHeaders.ContentType, HttpContentTypes.ApplicationJson);

            _strategy.CanHandle(_validJsonResponseTemplate)
                .Should().BeTrue();
        }

        [Fact]
        public void Can_Not_Handle_Valid_Json_Response_Body_With_Others_Content_Type_Header()
        {
            _validJsonResponseTemplate.Headers.Add(HttpHeaders.ContentType, "any/content-type");

            _strategy.CanHandle(_validJsonResponseTemplate)
                .Should().BeFalse();
        }

        [Fact]
        public void Returns_Response_With_Not_Indented_Valid_Json()
        {
            _validJsonResponseTemplate.Indented = false;

            var response = _strategy.GetResponseBodyFromTemplate(_validJsonResponseTemplate);

            response
                .Should().Be(UnindentedValidJson);
        }

        [Fact]
        public void Returns_Response_With_Indented_Valid_Json()
        {
            _validJsonResponseTemplate.Indented = true;

            var response = _strategy.GetResponseBodyFromTemplate(_validJsonResponseTemplate);

            response
                .Should().Be(IndentedValidJson);
        }

        [Fact]
        public void Returns_Response_With_Indented_Valid_Json_By_Default()
        {
            var response = _strategy.GetResponseBodyFromTemplate(_validJsonResponseTemplate);

            response
                .Should().Be(IndentedValidJson);
        }
    }
}
