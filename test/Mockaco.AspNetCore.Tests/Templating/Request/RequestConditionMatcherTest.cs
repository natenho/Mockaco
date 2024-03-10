using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace Mockaco.AspNetCore.Tests.Templating.Request
{
    public sealed class RequestConditionMatcherTest
    {
        private readonly IGlobalVariableStorage _globalVariableStorage;
        private readonly ITemplateTransformer _templateTransformer;
        private readonly IFakerFactory _fakerFactory;
        private readonly ILogger<RequestConditionMatcher> _logger;
        private readonly IRequestBodyFactory _requestBodyFactory;
        private readonly IMockacoContext _mockacoContext;

        public RequestConditionMatcherTest()
        {
            _templateTransformer = Moq.Mock.Of<ITemplateTransformer>();
            _fakerFactory = Moq.Mock.Of<IFakerFactory>();
            _requestBodyFactory = Moq.Mock.Of<IRequestBodyFactory>();
            _mockacoContext = Moq.Mock.Of<IMockacoContext>();
            _globalVariableStorage = Moq.Mock.Of<IGlobalVariableStorage>();
            _logger = Moq.Mock.Of<ILogger<RequestConditionMatcher>>();
        }

        [Fact]
        public async Task Condition_Does_Not_Match_On_Script_Error()
        {
            Moq.Mock.Get(_templateTransformer)
                .Setup(n => n.Transform(It.IsAny<IRawTemplate>(), It.IsAny<IScriptContext>()))
                .ThrowsAsync(new NotImplementedException());

            Moq.Mock.Get(_mockacoContext).Setup(c => c.Errors).Returns(new List<Error>());

            var conditionMatcher = new RequestConditionMatcher(
                _templateTransformer, _fakerFactory, _requestBodyFactory, _mockacoContext, _globalVariableStorage, _logger);

            var httpRequest = new Mock<HttpRequest>();
            httpRequest.Setup(h => h.HttpContext)
                .Returns(Moq.Mock.Of<HttpContext>(c => c.Request == httpRequest.Object));

            var rawTemplate = Moq.Mock.Of<IRawTemplate>();
            Moq.Mock.Get(rawTemplate).Setup(r => r.Content).Returns(@"");

            var mock = new Mock("GET", "/ping", rawTemplate, true);

            var isMatch = await conditionMatcher.IsMatch(httpRequest.Object, mock);

            isMatch.Should().Be(false);
        }
    }
}
