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
        [InlineData(0, 1000, 1000)]
        [InlineData(500, 1000, 1000)]
        [InlineData(1000, 1000, 1000)]
        [InlineData(1500, 1000, 1500)]
        [InlineData(2000, 1000, 2000)]
        public async Task Compensates_Request_Duration(int requestTime, int delay, int expectedDelay)
        {
            Moq.Mock.Get(_next).Setup(n => n.Invoke(It.IsAny<HttpContext>()))
                .Returns(async () => await Task.Delay(requestTime));

            Moq.Mock.Get(_mockacoContext).Setup(c => c.TransformedTemplate)
                .Returns(new Template() { Response = new ResponseTemplate() { Delay = delay } });

            var stopwatch = Stopwatch.StartNew();

            await _middleware.Invoke(_httpContext, _mockacoContext, _logger);

            stopwatch.Stop();

            stopwatch.ElapsedMilliseconds.Should().BeCloseTo(expectedDelay, 100);
        }
    }
}
