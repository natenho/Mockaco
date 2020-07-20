using Microsoft.Extensions.Logging;
using Mono.TextTemplating;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Mockaco
{
    public class MockProvider : IMockProvider
    {
        private List<Mock> _cache;
        private readonly List<(string TemplateName, string ErrorMessage)> _errors = new List<(string TemplateName, string Error)>();        
        private readonly IFakerFactory _fakerFactory;
        private readonly IRequestBodyFactory _requestBodyFactory;
        private readonly ITemplateProvider _templateProvider;
        private readonly ITemplateTransformer _templateTransformer;
        private readonly IGlobalVariableStorage _globalVariableStorage;
        private readonly ILogger<MockProvider> _logger;

        public MockProvider
            (IFakerFactory fakerFactory, 
            IRequestBodyFactory requestBodyFactory, 
            ITemplateProvider templateProvider, 
            ITemplateTransformer templateTransformer, 
            IGlobalVariableStorage globalVariableStorage,
            ILogger<MockProvider> logger)
        {
            _cache = new List<Mock>();
            _fakerFactory = fakerFactory;
            _requestBodyFactory = requestBodyFactory;
            _templateProvider = templateProvider;
            _templateProvider.OnChange += TemplateProviderChange;

            _templateTransformer = templateTransformer;
            _globalVariableStorage = globalVariableStorage;
            _logger = logger;
        }

        private async void TemplateProviderChange(object sender, EventArgs e)
        {
            await WarmUp();
        }

        //TODO: Fix potential thread-unsafe method
        public List<Mock> GetMocks()
        {
            return _cache;
        }

        public IEnumerable<(string TemplateName, string ErrorMessage)> GetErrors()
        {
            return _errors;
        }

        public async Task WarmUp()
        {
            var stopwatch = Stopwatch.StartNew();

            var warmUpScriptContext = new ScriptContext(_fakerFactory, _requestBodyFactory, _globalVariableStorage);

            const int defaultCapacity = 16;
            var mocks = new List<Mock>(_cache.Count > 0 ? _cache.Count : defaultCapacity);

            _errors.Clear();

            foreach (var rawTemplate in _templateProvider.GetTemplates())
            {
                try
                {
                    var existentCachedRoute = _cache.FirstOrDefault(cachedRoute => cachedRoute.RawTemplate.Hash == rawTemplate.Hash);

                    if (existentCachedRoute != default)
                    {
                        _logger.LogDebug("Using cached {0} ({1})", rawTemplate.Name, rawTemplate.Hash);

                        mocks.Add(existentCachedRoute);

                        continue;
                    }

                    _logger.LogDebug("Loading {0} ({1})", rawTemplate.Name, rawTemplate.Hash);

                    var template = await _templateTransformer.Transform(rawTemplate, warmUpScriptContext);
                    var mock = CreateMock(rawTemplate, template.Request);

                    mocks.Add(mock);

                    _logger.LogInformation("{method} {route} mapped from {templateName}", template.Request?.Method, template.Request?.Route, rawTemplate.Name);
                }
                catch (JsonReaderException ex)
                {
                    _errors.Add((rawTemplate.Name, $"Generated JSON is invalid - {ex.Message}"));
                    
                    _logger.LogWarning("Skipping {0}: Generated JSON is invalid - {1}", rawTemplate.Name, ex.Message);
                }
                catch (ParserException ex)
                {
                    _errors.Add((rawTemplate.Name, $"Script parser error - {ex.Message} {ex.Location}"));

                    _logger.LogWarning("Skipping {0}: Script parser error - {1} {2} ", rawTemplate.Name, ex.Message, ex.Location);
                }
                catch (Exception ex)
                {
                    _errors.Add((rawTemplate.Name, ex.Message));

                    _logger.LogWarning("Skipping {0}: {1}", rawTemplate.Name, ex.Message);
                }
            }

            _cache.Clear();

            _cache = mocks.OrderByDescending(r => r.HasCondition).ToList();

            _logger.LogTrace("{0} finished in {1} ms", nameof(WarmUp), stopwatch.ElapsedMilliseconds);
        }

        private static Mock CreateMock(IRawTemplate rawTemplate, RequestTemplate requestTemplate)
        {
            return new Mock(requestTemplate?.Method, requestTemplate?.Route, rawTemplate, requestTemplate?.Condition != default);
        }
    }
}
