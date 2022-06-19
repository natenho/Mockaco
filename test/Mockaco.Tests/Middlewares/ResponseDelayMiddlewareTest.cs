using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Mockaco.Tests.Middlewares
{
    public class ResponseDelayMiddlewareTest
    {
        private RequestDelegate _next;
        private IMockacoContext _mockacoContext;
        private ResponseDelayMiddleware _middleware;
        private HttpContext _httpContext;
        private ILogger<ResponseDelayMiddleware> _logger;

        public ResponseDelayMiddlewareTest()
        {
            _next = Moq.Mock.Of<RequestDelegate>();
            _mockacoContext = Moq.Mock.Of<IMockacoContext>();
            _middleware = new ResponseDelayMiddleware(_next);
            _httpContext = Moq.Mock.Of<HttpContext>();
            _logger = Moq.Mock.Of<ILogger<ResponseDelayMiddleware>>();
        }

        [Theory]
        [InlineData(1000, 1000)]
        [InlineData(2500, 2500)]
        [InlineData(3000, 3000)]
        public async Task Modifies_Request_Duration(int delay, int expectedDelay)
        {
            Moq.Mock.Get(_mockacoContext).Setup(c => c.TransformedTemplate)
                .Returns(new Template() { Response = new ResponseTemplate() { Delay = delay } });

            var stopwatch = Stopwatch.StartNew();

            await _middleware.Invoke(_httpContext, _mockacoContext, _logger);

            stopwatch.Stop();

            stopwatch.ElapsedMilliseconds.Should().BeCloseTo(expectedDelay, 100);
        }
    }
}
