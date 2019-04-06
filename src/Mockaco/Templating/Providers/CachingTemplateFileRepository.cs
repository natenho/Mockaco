using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Mockaco
{
    public sealed class CachingTemplateFileProvider : ITemplateProvider, IDisposable
    {
        public event EventHandler CacheInvalidated;

        private readonly IMemoryCache _memoryCache;
        private readonly ITemplateProvider _templateRepository;
        private readonly ILogger<CachingTemplateFileProvider> _logger;
        private readonly FileSystemWatcher _fileSystemWatcher;
        private CancellationTokenSource _resetCacheToken = new CancellationTokenSource();

        public CachingTemplateFileProvider(IMemoryCache memoryCache, TemplateFileProvider templateProvider, ILogger<CachingTemplateFileProvider> logger)
        {
            _memoryCache = memoryCache;
            _templateRepository = templateProvider;

            _fileSystemWatcher = new FileSystemWatcher("Mocks", "*.json");
            _fileSystemWatcher.Changed += MockChanged;
            _fileSystemWatcher.Created += MockChanged;
            _fileSystemWatcher.Deleted += MockChanged;
            _fileSystemWatcher.Renamed += MockChanged;            
            _fileSystemWatcher.IncludeSubdirectories = true;
            _fileSystemWatcher.EnableRaisingEvents = true;

            _logger = logger;
        }

        private void MockChanged(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation($"{e.FullPath} {e.ChangeType}");

            FlushCache();
        }

        public IEnumerable<IRawTemplate> GetTemplates()
        {
            return _memoryCache.GetOrCreate(string.Empty, e =>
            {
                e.RegisterPostEvictionCallback(PostEvictionCallback);
                e.AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token));

                return _templateRepository.GetTemplates();
            });
        }

        private void PostEvictionCallback(object key, object value, EvictionReason reason, object state)
        {
            _logger.LogDebug($"Cache invalidated because of {reason}");

            CacheInvalidated?.Invoke(this, EventArgs.Empty);
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

        public void Dispose()
        {
            _fileSystemWatcher.Dispose();
        }
    }
}