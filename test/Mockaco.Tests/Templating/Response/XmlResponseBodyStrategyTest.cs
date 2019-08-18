using FluentAssertions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Mockaco.Tests.Templating
{
    public class XmlResponseBodyStrategyTest
    {
        private const string UnindentedValidXml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root><hung bat=\"saw\">-820689514</hung><clay among=\"roof\"><wet>lunch</wet><yard either=\"event\"><product><!--green dry rose baby classroom thick-->174824199.0168128</product><![CDATA[express work bottle]]><exchange>-1202804739.2211328</exchange></yard></clay></root>";
        private const string IndentedValidXml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<root>\r\n  <hung bat=\"saw\">-820689514</hung>\r\n  <clay among=\"roof\">\r\n    <wet>lunch</wet>\r\n    <yard either=\"event\">\r\n      <product>\r\n        <!--green dry rose baby classroom thick-->174824199.0168128</product><![CDATA[express work bottle]]><exchange>-1202804739.2211328</exchange></yard>\r\n  </clay>\r\n</root>";

        private XmlResponseBodyStrategy _strategy;

        public XmlResponseBodyStrategyTest()
        {
            _strategy = new XmlResponseBodyStrategy();
        }

        [Fact]
        public void Can_Not_Handle_Response_Template_With_Default_Properties()
        {
            var responseTemplate = new ResponseTemplate();

            _strategy.CanHandle(responseTemplate)
                .Should().BeFalse();
        }

        [Fact]
        public void Can_Not_Handle_Response_Template_With_Other_Content_Type()
        {
            var responseTemplate = new ResponseTemplate();
            responseTemplate.Headers.Add(HttpHeaders.ContentType, "any/content-type");

            _strategy.CanHandle(responseTemplate)
                .Should().BeFalse();
        }

        [Fact]
        public void Can_Handle_Response_Template_With_Application_Xml_Content_Type()
        {
            var responseTemplate = new ResponseTemplate();
            responseTemplate.Headers.Add(HttpHeaders.ContentType, HttpContentTypes.ApplicationXml);

            _strategy.CanHandle(responseTemplate)
                .Should().BeTrue();
        }

        [Fact]
        public void Can_Handle_Response_Template_With_Text_Xml_Content_Type()
        {
            var responseTemplate = new ResponseTemplate();
            responseTemplate.Headers.Add(HttpHeaders.ContentType, HttpContentTypes.TextXml);

            _strategy.CanHandle(responseTemplate)
                .Should().BeTrue();
        }

        [Fact]
        public void Returns_Response_With_Not_Indented_Valid_Xml()
        {
            var responseTemplate = new ResponseTemplate();
            responseTemplate.Headers.Add(HttpHeaders.ContentType, HttpContentTypes.ApplicationXml);
            responseTemplate.Body = JToken.FromObject(IndentedValidXml);
            responseTemplate.Indented = false;

            var response = _strategy.GetResponseBodyFromTemplate(responseTemplate);

            response
                .Should().Be(UnindentedValidXml);
        }

        [Fact]
        public void Returns_Response_With_Indented_Valid_Xml()
        {
            var responseTemplate = new ResponseTemplate();
            responseTemplate.Headers.Add(HttpHeaders.ContentType, HttpContentTypes.ApplicationXml);
            responseTemplate.Body = JToken.FromObject(UnindentedValidXml);
            responseTemplate.Indented = true;

            var response = _strategy.GetResponseBodyFromTemplate(responseTemplate);

            response
                .Should().Be(IndentedValidXml);
        }

        [Fact]
        public void Returns_Response_With_Indented_Valid_Xml_By_Default()
        {
            var responseTemplate = new ResponseTemplate();
            responseTemplate.Headers.Add(HttpHeaders.ContentType, HttpContentTypes.ApplicationXml);
            responseTemplate.Body = JToken.FromObject(UnindentedValidXml);
            
            var response = _strategy.GetResponseBodyFromTemplate(responseTemplate);

            response
                .Should().Be(IndentedValidXml);
        }
    }
}
