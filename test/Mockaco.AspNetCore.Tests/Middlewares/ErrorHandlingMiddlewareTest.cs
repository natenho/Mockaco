using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            DefaultHttpContext httpContext = GetHttpContextForTest();

            var anyException = new InvalidOperationException("Any error");
            Moq.Mock.Get(_next).Setup(n => n.Invoke(It.IsAny<HttpContext>())).ThrowsAsync(anyException);

            var mockacoContext = Moq.Mock.Of<IMockacoContext>(c => c.Errors == new List<Error>());

            await _middleware.Invoke(httpContext, mockacoContext, _statusCodeOptions, _mockProvider, _logger);

            var body = await ReadResponseBody(httpContext);

            var bodyAsJToken = JToken.Parse(body);

            bodyAsJToken.Should().NotBeNullOrEmpty();
            bodyAsJToken.SelectToken("$[0].Exception.ClassName").ToString().Should().Be(anyException.GetType().FullName);
            bodyAsJToken.SelectToken("$[0].Exception.Message").ToString().Should().Be(anyException.Message);
            httpContext.Response.StatusCode.Should().Be(501);
            httpContext.Response.ContentType.Should().Be("application/json");
        }

        [Fact]
        public async Task Writes_Mockaco_Context_Errors_To_Response_Body()
        {
            var httpContext = GetHttpContextForTest();

            var anyError = new Error("any message");

            var mockacoContext = Moq.Mock.Of<IMockacoContext>(c => c.Errors == new List<Error> { anyError });

            await _middleware.Invoke(httpContext, mockacoContext, _statusCodeOptions, _mockProvider, _logger);

            var body = await ReadResponseBody(httpContext);

            var bodyAsJToken = JToken.Parse(body);

            bodyAsJToken.Should().NotBeNullOrEmpty();

            bodyAsJToken.SelectToken("$[0].Message").ToString().Should().Be(anyError.Message);

            httpContext.Response.StatusCode.Should().Be(501);
            httpContext.Response.ContentType.Should().Be("application/json");
        }

        [Fact]
        public async Task Includes_MockProvider_Errors_To_Response_Body_When_Any_Error_Occurs()
        {
            const string providerMessage = "any provider message";
            var httpContext = GetHttpContextForTest();

            Moq.Mock.Get(_mockProvider).Setup(m => m.GetErrors()).Returns(new[] { ("any/mock", providerMessage) }).Verifiable();

            var errors = new List<Error> { new Error("any message") };
            var mockacoContext = Moq.Mock.Of<IMockacoContext>(c => c.Errors == errors);

            await _middleware.Invoke(httpContext, mockacoContext, _statusCodeOptions, _mockProvider, _logger);

            Moq.Mock.Verify(Moq.Mock.Get(_mockProvider));

            errors.Should().HaveCount(2);
            errors.Any(e => e.Message.Contains(providerMessage)).Should().BeTrue();
        }

        private static DefaultHttpContext GetHttpContextForTest()
        {
            var httpContext = new DefaultHttpContext();

            httpContext.Response.Body = new MemoryStream();

            return httpContext;
        }

        private static async Task<string> ReadResponseBody(DefaultHttpContext httpContext)
        {
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);

            var streamReader = new StreamReader(httpContext.Response.Body);

            var body = await streamReader.ReadToEndAsync();

            return body;
        }
    }
}
