using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Mockaco.Processors;
using Mono.TextTemplating;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mockaco
{
    public class ReloadableRouter : IRouter
    {
        private static CancellationTokenSource _resetCacheToken = new CancellationTokenSource();
        private readonly IMemoryCache _memoryCache;
        private readonly ITemplateRepository _templateRepository;
        private readonly IApplicationBuilder _applicationBuilder;
        private readonly ILogger<ReloadableRouter> _logger;
        
        public ReloadableRouter(IMemoryCache memoryCache, ITemplateRepository templateRepository, IApplicationBuilder applicationBuilder, ILogger<ReloadableRouter> logger)
        {
            _memoryCache = memoryCache;
            _templateRepository = templateRepository;
            _templateRepository.CacheInvalidated += _templateRepository_CacheInvalidated;

            _applicationBuilder = applicationBuilder;
            _logger = logger;
        }

        private void _templateRepository_CacheInvalidated(object sender, EventArgs e)
        {
            FlushCache();
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            var router = GetRouter().GetAwaiter().GetResult();

            return router.GetVirtualPath(context);
        }

        public async Task RouteAsync(RouteContext context)
        {
            var router = await GetRouter();

            await router.RouteAsync(context);
        }

        private Task<IRouter> GetRouter()
        {
            return _memoryCache.GetOrCreateAsync(nameof(ReloadableRouter), async e =>
            {
                e.AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token));

                var routeBuilder = new RouteBuilder(_applicationBuilder);
                await ConfigureRoute(routeBuilder);

                var router = routeBuilder.Build();

                _logger.LogTrace("Router loaded");

                return router;
            });
        }

        private void FlushCache()
        {
            if (_resetCacheToken != null && !_resetCacheToken.IsCancellationRequested &&
                _resetCacheToken.Token.CanBeCanceled)
            {
                _resetCacheToken.Cancel();
                _resetCacheToken.Dispose();
            }
            _resetCacheToken = new CancellationTokenSource();
        }

        private async Task ConfigureRoute(IRouteBuilder routeBuilder)
        {
            foreach (var templateFile in _templateRepository.GetAll())
            {
                var template = await ProcessTemplate(routeBuilder.ServiceProvider, templateFile);

                if (template == null)
                {
                    continue;
                }

                var httpMethod = template.Request.Method?.ToString() ?? HttpMethod.Get.ToString();
                var route = template.Request.Route ?? string.Empty;

                routeBuilder.MapVerb(httpMethod, route, httpContext =>
                {
                    var templateProcessor = (ITemplateProcessor)httpContext.RequestServices.GetService(typeof(ITemplateProcessor));

                    return templateProcessor.ProcessResponse(httpContext);
                });
            }
        }

        private static async Task<Template> ProcessTemplate(IServiceProvider serviceProvider, TemplateFile templateFile)
        {
            Template template = null;

            var logger = serviceProvider.GetService<ILogger<Startup>>();

            try
            {
                var templateTransformer = serviceProvider.GetService<ITemplateTransformer>();
                var scriptContext = new ScriptContext();
                var parsedTemplate = await templateTransformer.Transform(templateFile.Content, scriptContext);

                template = JsonConvert.DeserializeObject<Template>(parsedTemplate);
            }
            catch (JsonReaderException ex)
            {
                logger.LogWarning("Skipping {0}: Generated JSON is invalid - {1}", templateFile.FileName, ex.Message);
            }
            catch (ParserException ex)
            {
                logger.LogWarning("Skipping {0}: Script parser error - {1} {2} ", templateFile.FileName, ex.Message, ex.Location);
            }
            catch (Exception ex)
            {
                logger.LogWarning("Skipping {0}: {1}", templateFile.FileName, ex.Message);
            }

            return template;
        }
    }
}