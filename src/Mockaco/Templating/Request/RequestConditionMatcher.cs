using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mockaco
{
    public class RequestConditionMatcher : IRequestMatcher
    {
        private readonly ITemplateTransformer _templateTransformer;
        private readonly IFakerFactory _fakerFactory;
        private readonly IRequestBodyFactory _requestBodyFactory;
        private readonly IMockacoContext _mockacoContext;
        private readonly IGlobalVariableStorage _globalVarialeStorage;
        private readonly ILogger _logger;

        public RequestConditionMatcher(
            ITemplateTransformer templateTransformer,
            IFakerFactory fakerFactory,
            IRequestBodyFactory requestBodyFactory,
            IMockacoContext mockacoContext,
            IGlobalVariableStorage globalVariableStoreage,
            ILogger<RequestConditionMatcher> logger)
        {
            _templateTransformer = templateTransformer;
            _fakerFactory = fakerFactory;
            _requestBodyFactory = requestBodyFactory;
            _mockacoContext = mockacoContext;
            _globalVarialeStorage = globalVariableStoreage;
            _logger = logger;
        }

        public async Task<bool> IsMatch(HttpRequest httpRequest, Mock mock)
        {
            var conditionMatcherScriptContext = new ScriptContext(_fakerFactory, _requestBodyFactory, _globalVarialeStorage);
            
            await AttachRequestToScriptContext(httpRequest.HttpContext, _mockacoContext, conditionMatcherScriptContext);

            if (_mockacoContext.Errors.Any())
            {
                return false;
            }

            await conditionMatcherScriptContext.AttachRouteParameters(httpRequest, mock);

            var template = await _templateTransformer.Transform(mock.RawTemplate, conditionMatcherScriptContext);

            var isMatch = template.Request?.Condition ?? true;

            return isMatch;
        }

        //TODO Remove redundant code
        private async Task AttachRequestToScriptContext(HttpContext httpContext, IMockacoContext mockacoContext, IScriptContext scriptContext)
        {
            try
            {
                await scriptContext.AttachRequest(httpContext.Request);
            }
            catch (Exception ex)
            {
                mockacoContext.Errors.Add(new Error("An error occurred while reading request", ex));

                _logger.LogWarning(ex, "An error occurred while reading request");

                return;
            }
        }
    }
}
