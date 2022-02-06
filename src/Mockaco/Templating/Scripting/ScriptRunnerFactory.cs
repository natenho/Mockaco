using Bogus;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Mockaco
{
    public class ScriptRunnerFactory : IScriptRunnerFactory
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<ScriptRunnerFactory> _logger;
        private readonly IOptionsMonitor<MockacoOptions> _options;

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
                _logger.LogTrace("Script cache hit");

                return runner;
            }

            runner = CreateRunner<TContext, TResult>(code);

            _cache.Set(code, runner);

            return runner;
        }

        public ScriptRunner<TResult> CreateRunner<TContext, TResult>(string code)
        {
            var stopWatch = Stopwatch.StartNew();

            var scriptOptions = ScriptOptions
                .Default
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
    }
}

