using MAB.DotIgnore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Polly;
using Polly.Retry;
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

        private const string MockIgnoreFileName = ".mockignore";
        private const string DefaultTemplateFolder = "Mocks";
        private const string DefaultTemplateSearchPattern = "*.json";

        private static readonly RetryPolicy _retryPolicy = Policy
            .Handle<IOException>()
            .WaitAndRetry(new[] 
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
            });

        private readonly ILogger<TemplateFileProvider> _logger;
        private readonly PhysicalFileProvider _fileProvider;
        private readonly IMemoryCache _memoryCache;        
        private CancellationTokenSource _resetCacheToken = new CancellationTokenSource();

        public TemplateFileProvider(IMemoryCache memoryCache, ILogger<TemplateFileProvider> logger)
        {
            _memoryCache = memoryCache;
            _fileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory(), ExclusionFilters.Hidden | ExclusionFilters.System);
            _logger = logger;

            KeepWatchingForFileChanges();
        }

        private void KeepWatchingForFileChanges()
        {
            var jsonChangeToken = _fileProvider.Watch($"{DefaultTemplateFolder}/**/{DefaultTemplateSearchPattern}");            
            jsonChangeToken.RegisterChangeCallback(TemplateFileModified, default);

            var mockIgnoreChangeToken = _fileProvider.Watch($"{DefaultTemplateFolder}/**/*{MockIgnoreFileName}");
            mockIgnoreChangeToken.RegisterChangeCallback(TemplateFileModified, default);
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
            _logger.LogDebug("Cache invalidated because of {reason}", reason);
        }

        private IEnumerable<IRawTemplate> LoadTemplatesFromDirectory()
        {
            var directory = new DirectoryInfo(DefaultTemplateFolder);

            foreach (var file in directory.GetFiles(DefaultTemplateSearchPattern, SearchOption.AllDirectories)
                                .OrderBy(f => f.FullName))
            {                
                if(ShouldIgnoreFile(file))
                {
                    _logger.LogDebug("{filePath} ignored using a {MockIgnoreFileName} file", Path.GetRelativePath(DefaultTemplateFolder, file.FullName), MockIgnoreFileName);
                    continue;
                }

                var name = Path.GetRelativePath(directory.FullName, file.FullName);
                var rawContent = string.Empty;

                _retryPolicy.Execute(() =>
                {
                    using (var stream = File.Open(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var streamReader = new StreamReader(stream))
                    {
                        rawContent = streamReader.ReadToEnd();
                    }
                });

                yield return new RawTemplate(name, rawContent);
            }
        }

        private static bool ShouldIgnoreFile(FileInfo file)
        {
            var mockIgnorePath = Path.Combine(file.DirectoryName, MockIgnoreFileName);

            if (!File.Exists(mockIgnorePath))
            {
                return false;
            }

            IgnoreList ignores = default;

            _retryPolicy.Execute(() =>
            {
                ignores = new IgnoreList(mockIgnorePath);
            });
                        
            return ignores.IsIgnored(file);
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