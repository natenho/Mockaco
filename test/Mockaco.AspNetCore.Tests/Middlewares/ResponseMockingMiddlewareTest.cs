using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Mockaco.Tests.Middlewares
{
    public class ResponseMockingMiddlewareTest
    {
        private readonly ResponseMockingMiddleware _middleware;

        public ResponseMockingMiddlewareTest()
        {
            var next = Moq.Mock.Of<RequestDelegate>();
            _middleware = new ResponseMockingMiddleware(next);
        }

        [Fact]
        public async Task Produces_Response_With_Default_Http_Status_When_Ommited_In_Template()
        {
            var defaultHttpStatusCode = HttpStatusCode.OK;
            var actualHttpStatusCode = default(int);

            var httpResponse = new Mock<HttpResponse>();            
            httpResponse.SetupSet(r => r.StatusCode = It.IsAny<int>()).Callback<int>(value => actualHttpStatusCode = value);
            httpResponse.Setup(r => r.Body).Returns(Moq.Mock.Of<Stream>());

            var httpContext = Moq.Mock.Of<HttpContext>(c => c.Response == httpResponse.Object);

            var templateWithOmmitedStatus = Moq.Mock.Of<Template>(t => t.Response == Moq.Mock.Of<ResponseTemplate>());
            var mockacoContext = Moq.Mock.Of<IMockacoContext>(c => c.TransformedTemplate == templateWithOmmitedStatus);

            var scriptContext = Moq.Mock.Of<IScriptContext>();
            var responseBodyFactory = Moq.Mock.Of<IResponseBodyFactory>();

            var optionsWithDefaulHttpStatusCode = Moq.Mock.Of<MockacoOptions>(o => o.DefaultHttpStatusCode == defaultHttpStatusCode);
            var mockacoOptionsSnapshot = Moq.Mock.Of<IOptionsSnapshot<MockacoOptions>>(s => s.Value == optionsWithDefaulHttpStatusCode);

            await _middleware.Invoke(httpContext, mockacoContext, scriptContext, responseBodyFactory, mockacoOptionsSnapshot);

            actualHttpStatusCode.Should()
                .Be((int)defaultHttpStatusCode);
        }
    }
}
