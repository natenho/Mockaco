using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Mockaco
{
    public sealed class TemplateFileProvider : ITemplateProvider, IDisposable
    {
        public event EventHandler OnChange;

        private readonly ILogger<TemplateFileProvider> _logger;
        private readonly PhysicalFileProvider _fileProvider;
        private readonly IMemoryCache _memoryCache;
        private IChangeToken _fileChangeToken;
        private CancellationTokenSource _resetCacheToken = new CancellationTokenSource();

        public TemplateFileProvider(IMemoryCache memoryCache, ILogger<TemplateFileProvider> logger)
        {
            _memoryCache = memoryCache;
            _fileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());
            _logger = logger;

            KeepWatchingForFileChanges();            
        }

        private void KeepWatchingForFileChanges()
        {
            _fileChangeToken = _fileProvider.Watch("Mocks/**/*.json");
            _fileChangeToken.RegisterChangeCallback(TemplateFileModified, default);
        }

        private void TemplateFileModified(object state)
        {
            _logger.LogInformation("File change detected");

            FlushCache();
            GetTemplates();

            OnChange?.Invoke(this, EventArgs.Empty);

            KeepWatchingForFileChanges();
        }

        public IEnumerable<IRawTemplate> GetTemplates()
        {
            return _memoryCache.GetOrCreate(
                nameof(TemplateFileProvider),
                e =>
                {
                    e.RegisterPostEvictionCallback(PostEvictionCallback);
                    e.AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token));

                    return LoadTemplatesFromDirectory();
                });
        }

        private void PostEvictionCallback(object key, object value, EvictionReason reason, object state)
        {
            _logger.LogDebug($"Cache invalidated because of {reason}");
        }

        private static IEnumerable<IRawTemplate> LoadTemplatesFromDirectory()
        {
            var directory = new DirectoryInfo("Mocks");

            foreach (var file in directory.GetFiles("*.json", SearchOption.AllDirectories)
                .OrderBy(f => f.FullName))
            {
                var name = Path.GetRelativePath(directory.FullName, file.FullName);

                var rawContent = File.ReadAllText(file.FullName);

                yield return new RawTemplate(name, rawContent);
            }
        }

        private void FlushCache()
        {
            if (_resetCacheToken?.IsCancellationRequested == false && _resetCacheToken.Token.CanBeCanceled)
            {
                _resetCacheToken.Cancel();
                _resetCacheToken.Dispose();
            }

            _resetCacheToken = new CancellationTokenSource();
        }

        public void Dispose()
        {
            _fileProvider.Dispose();
        }
    }
}