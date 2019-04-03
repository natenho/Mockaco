using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Mockaco.Processors;
using Mono.TextTemplating;

namespace Mockaco
{
    public class ReloadableRouter : IRouter
    {
        private static CancellationTokenSource _resetCacheToken = new CancellationTokenSource();
        private readonly ITemplateRepository _templateRepository;
        private readonly IApplicationBuilder _applicationBuilder;
        private IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());

        public ReloadableRouter(ITemplateRepository templateRepository, IApplicationBuilder applicationBuilder)
        {
            _templateRepository = templateRepository;
            _applicationBuilder = applicationBuilder;
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
            return _memoryCache.GetOrCreateAsync(string.Empty, async e =>
            {
                e.AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token));

                var routeBuilder = new RouteBuilder(_applicationBuilder);
                await ConfigureRoute(routeBuilder);
                return routeBuilder.Build();
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