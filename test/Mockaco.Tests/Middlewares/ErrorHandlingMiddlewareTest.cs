using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mockaco.Tests.Middlewares
{
    public class ErrorHandlingMiddlewareTest
    {
        private readonly IOptionsSnapshot<MockacoOptions> _statusCodeOptions;
        private readonly IMockProvider _mockProvider;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly ErrorHandlingMiddleware _middleware;
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddlewareTest()
        {
            _next = Moq.Mock.Of<RequestDelegate>();            
            _statusCodeOptions = Moq.Mock.Of<IOptionsSnapshot<MockacoOptions>>(o => o.Value == new MockacoOptions());
            _mockProvider = Moq.Mock.Of<IMockProvider>();
            _logger = Moq.Mock.Of<ILogger<ErrorHandlingMiddleware>>();
            _middleware = new ErrorHandlingMiddleware(_next);
        }

        [Fact]
        public async Task Writes_Unhandled_Exceptions_Thrown_In_Inner_Middlewares_As_Errors_In_Response_Body()
        {
            var written = false;
            var httpContext = CreateHttpResponseMock(() => written = true);

            Moq.Mock.Get(_next).Setup(n => n.Invoke(It.IsAny<HttpContext>())).ThrowsAsync(new InvalidOperationException("Any error"));
                        
            var mockacoContext = Moq.Mock.Of<IMockacoContext>(c => c.Errors == new List<Error>());
                       
            await _middleware.Invoke(httpContext, mockacoContext, _statusCodeOptions, _mockProvider, _logger);

            Moq.Mock.Verify();
            written.Should().BeTrue();
        }     

        [Fact]
        public async Task Writes_Mockaco_Context_Errors_To_Response_Body()
        {
            var written = false;
            var httpContext = CreateHttpResponseMock(() => written = true);        
                        
            var mockacoContext = Moq.Mock.Of<IMockacoContext>(c => c.Errors == new List<Error>() { new Error("any message") });
            
            await _middleware.Invoke(httpContext, mockacoContext, _statusCodeOptions, _mockProvider, _logger);

            Moq.Mock.Verify();
            written.Should().BeTrue();
        }

        [Fact]
        public async Task Includes_MockProvider_Errors_To_Response_Body_When_Any_Error_Occurs()
        {
            var written = false;
            var providerMessage = "any provider message";
            var httpContext = CreateHttpResponseMock(() => written = true);

            Moq.Mock.Get(_mockProvider).Setup(m => m.GetErrors()).Returns(new[] { ("any/mock", providerMessage) }).Verifiable();

            var errors = new List<Error>() { new Error("any message") };
            var mockacoContext = Moq.Mock.Of<IMockacoContext>(c => c.Errors == errors);

            await _middleware.Invoke(httpContext, mockacoContext, _statusCodeOptions, _mockProvider, _logger);

            Moq.Mock.Verify();
            written.Should().BeTrue();
            errors.Should().HaveCount(2);
            errors.Any(e => e.Message.Contains(providerMessage)).Should().BeTrue();
        }

        private static HttpContext CreateHttpResponseMock(Action writeAsyncCallback)
        {
            var httpResponse = new Moq.Mock<HttpResponse>();
            httpResponse
                .Setup(r => r.Body.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Callback((byte[] data, int offset, int length, CancellationToken token) => writeAsyncCallback.Invoke());

            var httpContext = Moq.Mock.Of<HttpContext>(c => c.Response == httpResponse.Object);

            return httpContext;
        }
    }
}
