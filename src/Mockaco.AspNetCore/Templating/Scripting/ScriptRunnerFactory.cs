using Bogus;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Mockaco
{
    internal class ScriptRunnerFactory : IScriptRunnerFactory
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<ScriptRunnerFactory> _logger;
        private readonly IOptionsMonitor<MockacoOptions> _options;
        private MissingResolver _missingResolver;

        public ScriptRunnerFactory(ILogger<ScriptRunnerFactory> logger, IOptionsMonitor<MockacoOptions> options)
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _logger = logger;
            _options = options;
        }

        public Task<TResult> Invoke<TContext, TResult>(TContext context, string code)
        {
            var runner = GetOrCreateRunner<TContext, TResult>(code);
            
            return runner.Invoke(context);            
        }

        private ScriptRunner<TResult> GetOrCreateRunner<TContext, TResult>(string code)
        {
            if (_cache.TryGetValue<ScriptRunner<TResult>>(code, out var runner))
            {
                _logger.LogTrace("Cache hit");

                return runner;
            }

            runner = CreateRunner<TContext, TResult>(code);

            _cache.Set(code, runner);

            return runner;
        }

        public ScriptRunner<TResult> CreateRunner<TContext, TResult>(string code)
        {
            var stopWatch = Stopwatch.StartNew();

            if (_missingResolver == null) _missingResolver = new MissingResolver(_cache);

            var scriptOptions = ScriptOptions
                .Default
                .WithMetadataResolver(_missingResolver)
                .AddReferences(
                    typeof(Faker).Assembly,
                    typeof(ScriptRunnerFactory).Assembly)
                .AddReferences(_options.CurrentValue.References)
                .AddImports(
                    "System",
                    "System.Linq",
                    "System.Collections.Generic",
                    "System.Text.RegularExpressions",
                    typeof(Faker).Namespace,
                    typeof(ScriptRunnerFactory).Namespace,
                    "Newtonsoft.Json",
                    "Newtonsoft.Json.Linq")
                .AddImports(_options.CurrentValue.Imports)
                .WithOptimizationLevel(OptimizationLevel.Release);
            
            var script = CSharpScript.Create<TResult>(
                code,
                globalsType: typeof(TContext),
                options: scriptOptions);
            
            var runner = script.CreateDelegate();
            
            _logger.LogTrace("Created runner in {elapsedTime} milliseconds", stopWatch.ElapsedMilliseconds);

            return runner;
        }

        /// <summary>
        /// This class is used to reduce high memory usage during startup
        /// </summary>
        private class MissingResolver : MetadataReferenceResolver
        {
            public MissingResolver(IMemoryCache cache)
            {
                _cache = cache;
            }

            private readonly IMemoryCache _cache;

            public override bool Equals(object other)
            {
                return ScriptMetadataResolver.Default.Equals(other);
            }

            public override int GetHashCode()
            {
                return ScriptMetadataResolver.Default.GetHashCode();
            }

            public override bool ResolveMissingAssemblies => ScriptMetadataResolver.Default.ResolveMissingAssemblies;

            public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string baseFilePath, MetadataReferenceProperties properties)
            {
                var key = $"ResolveReference {reference} {baseFilePath} {properties.GetHashCode()}";

                return _cache.GetOrCreate(key, (e) =>
                {
                    e.SlidingExpiration = TimeSpan.FromSeconds(10);
                    return ScriptMetadataResolver.Default.ResolveReference(reference, baseFilePath, properties);
                });
            }

            public override PortableExecutableReference ResolveMissingAssembly(MetadataReference definition, AssemblyIdentity referenceIdentity)
            {
                var key = $"ResolveMissingAssembly {definition.Display} {referenceIdentity}";

                return _cache.GetOrCreate(key, (e) =>
                {
                    e.SlidingExpiration = TimeSpan.FromSeconds(10);
                    return ScriptMetadataResolver.Default.ResolveMissingAssembly(definition, referenceIdentity);
                });
            }
        }

    }
}

