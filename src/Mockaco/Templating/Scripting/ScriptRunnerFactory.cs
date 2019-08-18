using Bogus;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Mockaco
{
    public class ScriptRunnerFactory : IScriptRunnerFactory
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<ScriptRunnerFactory> _logger;

        public ScriptRunnerFactory(ILogger<ScriptRunnerFactory> logger)
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _logger = logger;
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

            runner = CreateRunner<TContext, TResult>(code, out runner);

            _cache.Set(code, runner);

            return runner;
        }

        public ScriptRunner<TResult> CreateRunner<TContext, TResult>(string code, out ScriptRunner<TResult> runner)
        {
            var stopWatch = Stopwatch.StartNew();

            var scriptOptions = ScriptOptions
                .Default
                .WithReferences(
                    typeof(Faker).Assembly,
                    typeof(ScriptRunnerFactory).Assembly)
                .WithImports(
                    "System",
                    "System.Linq",
                    "System.Collections.Generic",
                    "System.Text.RegularExpressions",
                    typeof(Faker).Namespace,
                    typeof(ScriptRunnerFactory).Namespace,
                    "Newtonsoft.Json",
                    "Newtonsoft.Json.Linq");

            var script = CSharpScript.Create<TResult>(
                code,
                globalsType: typeof(TContext),
                options: scriptOptions);

            runner = script.CreateDelegate();

            _logger.LogTrace("Created runner in {elapsedTime} milliseconds", stopWatch.ElapsedMilliseconds);

            return runner;
        }
    }
}

