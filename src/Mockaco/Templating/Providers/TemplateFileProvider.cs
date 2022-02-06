using MAB.DotIgnore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Polly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mockaco
{
    public sealed class TemplateFileProvider : ITemplateProvider, IDisposable
    {
        public event EventHandler OnChange;

        private const string MockIgnoreFileName = ".mockignore";
        private const string DefaultTemplateSearchPattern = "*.json";

        private readonly ILogger<TemplateFileProvider> _logger;
        
        private readonly IMemoryCache _memoryCache;
        private PhysicalFileProvider _fileProvider;
        private CancellationTokenSource _resetCacheToken = new CancellationTokenSource();

        public TemplateFileProvider(IOptionsMonitor<TemplateFileProviderOptions> options, IWebHostEnvironment webHostEnvironment, IMemoryCache memoryCache, ILogger<TemplateFileProvider> logger)
        {            
            _memoryCache = memoryCache;
            _logger = logger;

            SetMockRootPath(options.CurrentValue.Path);
            KeepWatchingForFileChanges();

            options.OnChange(options => {
                SetMockRootPath(options.Path);
                ResetCacheAndNotifyChange();
            });
        }

        private void SetMockRootPath(string path)
        {
            try
            {
                if(_fileProvider?.Root.Equals(path) == true)
                {
                    return;
                }

                var fullPath = Path.IsPathRooted(path) 
                    ? path 
                    : Path.Combine(Directory.GetCurrentDirectory(), path);
                
                var fileProvider = new PhysicalFileProvider(fullPath, ExclusionFilters.Hidden | ExclusionFilters.System);

                _fileProvider?.Dispose();
                _fileProvider = fileProvider;

                _logger.LogInformation("Mock path: {fullPath}", fullPath);
            }            
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting mock root path");
            }
        }

        private void ResetCacheAndNotifyChange()
        {
            FlushCache();
            GetTemplates();

            Task.Factory.StartNew(() => OnChange(this, EventArgs.Empty));

            KeepWatchingForFileChanges();
        }

        private void KeepWatchingForFileChanges()
        {
            if(_fileProvider == null)
            {
                return;
            }

            var jsonChangeToken = _fileProvider.Watch($"**/{DefaultTemplateSearchPattern}");
            jsonChangeToken.RegisterChangeCallback(TemplateFileModified, default);

            var mockIgnoreChangeToken = _fileProvider.Watch($"**/*{MockIgnoreFileName}");
            mockIgnoreChangeToken.RegisterChangeCallback(TemplateFileModified, default);
        }

        private void TemplateFileModified(object state)
        {
            _logger.LogInformation("File change detected");

            ResetCacheAndNotifyChange();
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
            _logger.LogDebug("Mock files cache invalidated because of {reason}", reason);
        }

        private IEnumerable<IRawTemplate> LoadTemplatesFromDirectory()
        {
            if(_fileProvider == null)
            {
                yield break;
            }

            var directory = new DirectoryInfo(_fileProvider.Root);

            foreach (var file in directory.GetFiles(DefaultTemplateSearchPattern, SearchOption.AllDirectories)
                                .OrderBy(f => f.FullName))
            {
                var relativeFilePath = Path.GetRelativePath(_fileProvider.Root, file.FullName);

                if (ShouldIgnoreFile(file))
                {
                    _logger.LogDebug("{relativeFilePath} ignored using a {MockIgnoreFileName} file", relativeFilePath, MockIgnoreFileName);
                    continue;
                }

                var name = Path.GetRelativePath(directory.FullName, file.FullName);
                var rawContent = string.Empty;

                var rawTemplate = WrapWithFileExceptionHandling(relativeFilePath, () =>
                {
                    using var stream = File.Open(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    using var streamReader = new StreamReader(stream);
                    rawContent = streamReader.ReadToEnd();
                    return new RawTemplate(name, rawContent, file.LastWriteTimeUtc);
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

            if (ignores == default)
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
            _fileProvider?.Dispose();
        }
    }
}