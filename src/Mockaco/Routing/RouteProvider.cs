using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Mockaco.Processors;
using Mono.TextTemplating;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Mockaco.Routing
{
    public class RouteProvider : IRouteProvider
    {
        private List<Route> _cache;
        private readonly List<(string TemplateName, string ErrorMessage)> _errors = new List<(string TemplateName, string Error)>();        
        private readonly IFakerFactory _fakerFactory;
        private readonly ITemplateProvider _templateProvider;
        private readonly ITemplateTransformer _templateTransformer;
        private readonly ILogger<RouteProvider> _logger;

        public RouteProvider(IFakerFactory fakerFactory, ITemplateProvider templateProvider, ITemplateTransformer templateTransformer, ILogger<RouteProvider> logger)
        {
            _cache = new List<Route>();
            _fakerFactory = fakerFactory;

            _templateProvider = templateProvider;
            _templateProvider.OnChange += TemplateProviderChange;

            _templateTransformer = templateTransformer;

            _logger = logger;
        }

        private async void TemplateProviderChange(object sender, EventArgs e)
        {
            await WarmUp();
        }

        //TODO: Fix potential thread-unsafe method
        public List<Route> GetRoutes()
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

            var nullScriptContext = new ScriptContext(_fakerFactory);

            const int defaultCapacity = 16;
            var routes = new List<Route>(_cache.Count > 0 ? _cache.Count : defaultCapacity);

            _errors.Clear();

            foreach (var rawTemplate in _templateProvider.GetTemplates())
            {
                try
                {
                    var existentCachedRoute = _cache.FirstOrDefault(cachedRoute => cachedRoute.RawTemplate.Hash == rawTemplate.Hash);

                    if (existentCachedRoute != default)
                    {
                        _logger.LogInformation("Using cached {0} ({1})", rawTemplate.Name, rawTemplate.Hash);

                        routes.Add(existentCachedRoute);

                        continue;
                    }

                    _logger.LogInformation("Loading {0} ({1})", rawTemplate.Name, rawTemplate.Hash);

                    var template = await _templateTransformer.Transform(rawTemplate, nullScriptContext);

                    routes.Add(new Route(template.Request.Method, template.Request.Route, rawTemplate, template.Request.Condition.HasValue));

                    _logger.LogInformation("Mapped {0} to {1} {2}", rawTemplate.Name, template.Request.Method, template.Request.Route);
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

            _cache = routes.OrderByDescending(r => r.HasCondition).ToList();

            _logger.LogTrace("{0} finished in {1} ms", nameof(WarmUp), stopwatch.ElapsedMilliseconds);
        }
    }
}
