using Microsoft.Extensions.Logging;
using Mono.TextTemplating;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mockaco
{
    public class MockProvider : IMockProvider
    {
        private ConcurrentDictionary<string, Mock> _cache = new();
        private readonly ConcurrentDictionary<string, (IRawTemplate, string)> _badTemplateHashes = new();
        private readonly ConcurrentBag<(string TemplateName, string ErrorMessage)> _errors = new();
        private readonly IFakerFactory _fakerFactory;
        private readonly IRequestBodyFactory _requestBodyFactory;
        private readonly ITemplateProvider _templateProvider;
        private readonly ITemplateTransformer _templateTransformer;
        private readonly IGlobalVariableStorage _globalVariableStorage;
        private readonly ILogger<MockProvider> _logger;

        private CancellationTokenSource _cancellationTokenSource;
        private AutoResetEvent _autoResetEvent = new(true);
        private SemaphoreSlim _semaphoreSlim = new(1, 1);

        public MockProvider
            (IFakerFactory fakerFactory,
            IRequestBodyFactory requestBodyFactory,
            ITemplateProvider templateProvider,
            ITemplateTransformer templateTransformer,
            IGlobalVariableStorage globalVariableStorage,
            ILogger<MockProvider> logger)
        {
            _fakerFactory = fakerFactory;
            _requestBodyFactory = requestBodyFactory;
            _templateProvider = templateProvider;
            _templateProvider.OnChange += TemplateProviderChange;

            _templateTransformer = templateTransformer;
            _globalVariableStorage = globalVariableStorage;
            _logger = logger;
        }

        private void TemplateProviderChange(object sender, EventArgs e)
        {
            _ = BuildCache();
        }

        public IEnumerable<Mock> GetMocks()
        {
            return _cache
                .Select(m => m.Value)
                .OrderBy(m => m.RawTemplate.Name)
                .ThenByDescending(m => m.HasCondition);
        }

        public IEnumerable<(string TemplateName, string ErrorMessage)> GetErrors()
        {
            return _errors;
        }

        public async Task BuildCache()
        {
            _cancellationTokenSource?.Cancel();

            await _semaphoreSlim.WaitAsync();

            var stopwatch = Stopwatch.StartNew();

            try
            {
                _cancellationTokenSource?.Token.ThrowIfCancellationRequested();

                _cancellationTokenSource = new CancellationTokenSource();

                _errors.Clear();

                var templates = _templateProvider
                    .GetTemplates()
                    .ToList();

                RemoveMissingTemplatesFromCache(templates, _cancellationTokenSource);

                var recentlyModifiedTemplates = templates.Where(_ => _.LastModified > DateTime.Now.AddMinutes(-15));
                BuildCacheFromTemplates(recentlyModifiedTemplates, _cancellationTokenSource);

                BuildCacheFromTemplates(templates.Except(recentlyModifiedTemplates), _cancellationTokenSource);

                _logger.LogDebug("{0} loaded {1} mocks in {2} ms", nameof(MockProvider), _cache.Count, stopwatch.ElapsedMilliseconds);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Changes were detected while building cache");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building mock cache");
            }
            finally
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
                _semaphoreSlim.Release();
            }
        }

        private void RemoveMissingTemplatesFromCache(IEnumerable<IRawTemplate> templates, CancellationTokenSource cancellationTokenSource)
        {
            var parallelOptions = new ParallelOptions
            {
                CancellationToken = cancellationTokenSource.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            Parallel.ForEach(_cache, parallelOptions, cacheItem =>
                {
                    if (templates.Any(template => template.Hash == cacheItem.Key))
                    {
                        return;
                    }

                    if (!_cache.TryRemove(cacheItem.Key, out var removedMock))
                    {
                        _logger.LogWarning("Could not remove {0} from cache", cacheItem.Value.RawTemplate);
                        return;
                    }

                    _logger.LogDebug("Removed {0} from cache", removedMock.RawTemplate);
                });
        }

        private void BuildCacheFromTemplates(IEnumerable<IRawTemplate> templates, CancellationTokenSource _cancellationTokenSource)
        {
            var warmUpScriptContext = new ScriptContext(_fakerFactory, _requestBodyFactory, _globalVariableStorage);

            var parallelOptions = new ParallelOptions
            {
                CancellationToken = _cancellationTokenSource.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            Parallel.ForEach(templates, parallelOptions, async rawTemplate =>
            {
                try
                {
                    if (_badTemplateHashes.TryGetValue(rawTemplate.Hash, out (IRawTemplate RawTemplate, string ErrorMessage) failedRawTemplate))
                    {
                        _logger.LogWarning("Skipping {0}: {1}", failedRawTemplate.RawTemplate.Name, failedRawTemplate.ErrorMessage);

                        return;
                    }

                    _cache.TryGetValue(rawTemplate.Hash, out var cachedMock);

                    if (cachedMock != default)
                    {
                        _logger.LogDebug("Using cached {0}", rawTemplate);

                        return;
                    }

                    _logger.LogDebug("Loading {0}", rawTemplate);

                    var template = await _templateTransformer.Transform(rawTemplate, warmUpScriptContext);
                    var mock = CreateMock(rawTemplate, template.Request);

                    if (!_cache.TryAdd(rawTemplate.Hash, mock))
                    {
                        _logger.LogWarning("Duplicated mocks: {0} and {1}", _cache[rawTemplate.Hash].RawTemplate, rawTemplate);
                        return;
                    }

                    _logger.LogInformation("{method} {route} mapped from {templateName}", template.Request?.Method, template.Request?.Route, rawTemplate.Name);
                }
                catch (JsonReaderException ex)
                {
                    var message = $"Generated JSON is invalid - {ex.Message}";
                    _errors.Add((rawTemplate.Name, message));

                    _logger.LogWarning("Skipping {0}: Generated JSON is invalid - {1}", rawTemplate.Name, message);

                    _badTemplateHashes.TryAdd(rawTemplate.Hash, (rawTemplate, message));
                }
                catch (ParserException ex)
                {
                    var message = $"Script parser error - {ex.Message} {ex.Location}";

                    _errors.Add((rawTemplate.Name, message));

                    _logger.LogWarning("Skipping {0}: Script parser error - {1}", rawTemplate.Name, message);

                    _badTemplateHashes.TryAdd(rawTemplate.Hash, (rawTemplate, message));
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _errors.Add((rawTemplate.Name, ex.Message));

                    _logger.LogWarning("Skipping {0}: {1}", rawTemplate.Name, ex.Message);

                    _badTemplateHashes.TryAdd(rawTemplate.Hash, (rawTemplate, ex.Message));
                }
            });
        }

        private static Mock CreateMock(IRawTemplate rawTemplate, RequestTemplate requestTemplate)
        {
            return new Mock(requestTemplate?.Method, requestTemplate?.Route, rawTemplate, requestTemplate?.Condition != default);
        }
    }
}
