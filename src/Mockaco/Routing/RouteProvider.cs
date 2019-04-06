using Microsoft.Extensions.Logging;
using Mockaco.Processors;
using Mono.TextTemplating;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mockaco.Routing
{
    public class RouteProvider : IRouteProvider
    {
        private List<Route> _routes;
        private readonly ITemplateProvider _templateProvider;
        private readonly ITemplateTransformer _templateTransformer;
        private readonly ILogger<RouteProvider> _logger;

        public RouteProvider(ITemplateProvider templateProvider, ITemplateTransformer templateTransformer, ILogger<RouteProvider> logger)
        {
            _routes = new List<Route>();
            _templateProvider = templateProvider;
            _templateTransformer = templateTransformer;
            _logger = logger;
        }

        public List<Route> GetRoutes()
        {
            return _routes;
        }

        public async Task WarmUp()
        {
            var blankScriptContext = new ScriptContext();

            foreach (var rawTemplate in _templateProvider.GetTemplates())
            {
                try
                {
                    _logger.LogInformation("Loading {0}", rawTemplate.Name);

                    var template = await _templateTransformer.Transform(rawTemplate, blankScriptContext);

                    _routes.Add(new Route(template.Request.Method, template.Request.Route, rawTemplate));

                    _logger.LogInformation("Mapped {0} to {1} {2}", rawTemplate.Name, template.Request.Method, template.Request.Route);
                }
                catch (JsonReaderException ex)
                {
                    _logger.LogWarning("Skipping {0}: Generated JSON is invalid - {1}", rawTemplate.Name, ex.Message);
                }
                catch (ParserException ex)
                {
                    _logger.LogWarning("Skipping {0}: Script parser error - {1} {2} ", rawTemplate.Name, ex.Message, ex.Location);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Skipping {0}: {1}", rawTemplate.Name, ex.Message);
                }
            }
        }
    }
}
