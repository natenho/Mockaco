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
                var relativeFilePath = Path.GetRelativePath(DefaultTemplateFolder, file.FullName);

                if (ShouldIgnoreFile(file))
                {
                    _logger.LogDebug("{relativeFilePath} ignored using a {MockIgnoreFileName} file", relativeFilePath, MockIgnoreFileName);
                    continue;
                }

                var name = Path.GetRelativePath(directory.FullName, file.FullName);
                var rawContent = string.Empty;
                                
                var rawTemplate = WrapWithFileExceptionHandling(relativeFilePath, () =>
                {
                    using (var stream = File.Open(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var streamReader = new StreamReader(stream))
                    {
                        rawContent = streamReader.ReadToEnd();
                        return new RawTemplate(name, rawContent);
                    }
                });

                if (rawTemplate != default)
                {
                    yield return rawTemplate;
                }
            }
        }

        private bool ShouldIgnoreFile(FileInfo file)
        {
            var mockIgnorePath = Path.Combine(file.DirectoryName, MockIgnoreFileName);

            if (!File.Exists(mockIgnorePath))
            {
                return false;
            }

            var ignores = WrapWithFileExceptionHandling(mockIgnorePath, () =>
            {
                return new IgnoreList(mockIgnorePath);
            });

            if(ignores == default)
            {
                return false;
            }

            return ignores.IsIgnored(file);
        }

        private T WrapWithFileExceptionHandling<T>(string path, Func<T> action)
        {
            void log(Exception ex) => _logger.LogWarning("Could not load {path} due to {exceptionType}: {exceptionMessage}", path, ex.GetType(), ex.Message);

            try
            {
                return Policy.Handle<IOException>()
                        .WaitAndRetry(sleepDurations: new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3) }, 
                                      onRetry: (ex, _) => log(ex))
                        .Execute(action);
            }
            catch (Exception ex)
            {
                log(ex);
            }

            return default;
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