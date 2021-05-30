using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace Mockaco.Tests.Middlewares
{
    public class RequestMatchingMiddlewareTest
    {
        private readonly RequestMatchingMiddleware _middleware;

        public RequestMatchingMiddlewareTest()
        {
            var next = Moq.Mock.Of<RequestDelegate>();
            var logger = Moq.Mock.Of<ILogger<RequestMatchingMiddleware>>();
            _middleware = new RequestMatchingMiddleware(next, logger);
        }

        [Fact]
        public async Task Attaches_Request_Parameters_To_Be_Accessible_Via_ScriptContext()
        {
            var expectedUri = new Uri("http://127.0.0.1");
            var expectedBody = Encoding.UTF8.GetBytes("TestBodyData");

            var httpRequest = new Moq.Mock<HttpRequest>();
            httpRequest.Setup(h => h.Scheme).Returns(expectedUri.Scheme);
            httpRequest.Setup(h => h.Host).Returns(new HostString(expectedUri.Host));
            httpRequest.Setup(h => h.Query).Returns(new QueryCollection());
            httpRequest.Setup(h => h.Headers).Returns(new HeaderDictionary());

            using (var bodyStream = new MemoryStream(expectedBody))
            {
                httpRequest.Setup(h => h.Body).Returns(bodyStream);

                var httpContext = Moq.Mock.Of<HttpContext>(c => c.Request == httpRequest.Object && c.Connection.RemoteIpAddress == IPAddress.Loopback);
                var mockacoContext = Moq.Mock.Of<IMockacoContext>(c => c.Errors == new List<Error>());

                var scriptContext = new Moq.Mock<IScriptContext>();
                scriptContext.Setup(c => c.AttachRequest(httpRequest.Object)).Verifiable();

                var mockProvider = new Moq.Mock<IMockProvider>();
                mockProvider.Setup(p => p.GetMocks()).Returns(new List<Mock>());

                var templateTransformer = Moq.Mock.Of<ITemplateTransformer>();
                var requestMatchers = Moq.Mock.Of<IEnumerable<IRequestMatcher>>();
                var memoryCache = Moq.Mock.Of<IMemoryCache>();
                var options = Moq.Mock.Of<IOptions<MockacoOptions>>();

                await _middleware.Invoke(httpContext, mockacoContext, scriptContext.Object, mockProvider.Object, templateTransformer, requestMatchers, memoryCache, options);

                Moq.Mock.Verify(scriptContext);
            }
        }
    }
}
