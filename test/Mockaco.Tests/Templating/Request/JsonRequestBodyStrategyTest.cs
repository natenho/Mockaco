using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Mockaco.Tests.Templating.Request
{
    public sealed class JsonRequestBodyStrategyTest : IDisposable
    {
        private MemoryStream _bodyStream;

        [Theory]
        [InlineData("[{\"A\":\"XYZ\",\"B\":\"99999999\"}]", "$[0].A", JTokenType.Array, "XYZ")]
        [InlineData("{\"A\":\"XYZ\",\"B\":\"99999999\"}", "$.A", JTokenType.Object, "XYZ")]
        [InlineData("\"XYZ\"", "$", JTokenType.String, "XYZ")]
        [InlineData("null", "$", JTokenType.Null, "")]
        [InlineData("", "$", JTokenType.Object, "{}")]
        public async Task Attaches_Request_With_Json_Body_With_Type(
            string givenBody,
            string jsonPath,
            JTokenType expectedJsonType,
            string expectedJsonValue)
        {
            var httpRequest = PrepareHttpRequest(givenBody);

            var bodyStrategy = new JsonRequestBodyStrategy();
            var actualBodyAsJson = await bodyStrategy.ReadBodyAsJson(httpRequest.Object);

            actualBodyAsJson.Type
                .Should()
                .Be(expectedJsonType);

            actualBodyAsJson.SelectToken(jsonPath).ToString()
                .Should()
                .Be(expectedJsonValue);
        }

        [Theory]
        [InlineData("x")]
        [InlineData("<>")]
        [InlineData("{ ab }")]
        public void Throws_Exception_When_Request_Has_Invalid_Json(string givenBody)
        {
            var httpRequest = PrepareHttpRequest(givenBody);

            var bodyStrategy = new JsonRequestBodyStrategy();

            bodyStrategy.Invoking(async _ => await _.ReadBodyAsJson(httpRequest.Object))
                .Should()
                .Throw<JsonReaderException>();
        }

        private Moq.Mock<HttpRequest> PrepareHttpRequest(string givenBody)
        {
            var httpRequest = new Moq.Mock<HttpRequest>();
            var bodyBuffer = Encoding.UTF8.GetBytes(givenBody);
            _bodyStream = new MemoryStream(bodyBuffer);

            httpRequest.Setup(h => h.Body).Returns(_bodyStream);

            return httpRequest;
        }

        public void Dispose() => _bodyStream.Dispose();
    }
}