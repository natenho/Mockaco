using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Mockaco.Tests.Middlewares
{
    public class RequestMatchingMiddlewareTest
    {
        private readonly ILogger<RequestMatchingMiddleware> _logger;
        private readonly RequestMatchingMiddleware _middleware;
        private readonly RequestDelegate _next;

        public RequestMatchingMiddlewareTest()
        {
            _next = Moq.Mock.Of<RequestDelegate>();
            _logger = Moq.Mock.Of<ILogger<RequestMatchingMiddleware>>();
            _middleware = new RequestMatchingMiddleware(_next, _logger);
        }

        [Fact]
        public async Task Attaches_Request_Parameters_To_Be_Accessible_Via_ScriptContext()
        {
            var expectedUri = new Uri("http://127.0.0.1");
            var expectedBody = Encoding.UTF8.GetBytes("TestBodyData");

            var httpRequest = new Moq.Mock<HttpRequest>();
            httpRequest.Setup(h => h.Scheme).Returns(expectedUri.Scheme.ToString());
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

                await _middleware.Invoke(httpContext, mockacoContext, scriptContext.Object, mockProvider.Object, templateTransformer, requestMatchers);
            }
            
            Moq.Mock.Verify();            
        }

    }
}
