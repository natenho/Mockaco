using Bogus;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Reflection;
using System.Threading.Tasks;

namespace Mockore
{
    public class ScriptEvaluator<TContext, TResult>
        where TContext : class
    {
        private readonly string _code;
        private ScriptRunner<TResult> _runner;

        public object Data { get; }

        public ScriptEvaluator(string code, object data = null)
        {
            Data = data;
            _code = code;
        }

        public void Compile()
        {
            if (_runner == null)
            {
                var scriptOptions = ScriptOptions.Default;

                // Add reference to mscorlib
                var mscorlib = typeof(object).GetTypeInfo().Assembly;
                var systemCore = typeof(System.Linq.Enumerable).GetTypeInfo().Assembly;

                var references = new[] { mscorlib, systemCore };
                scriptOptions = scriptOptions.AddReferences(references);
                scriptOptions = scriptOptions.AddReferences(typeof(Faker).Assembly, typeof(ScriptEvaluator<,>).Assembly);

                scriptOptions = scriptOptions.AddImports("System");
                scriptOptions = scriptOptions.AddImports("System.Linq");
                scriptOptions = scriptOptions.AddImports("System.Collections.Generic");
                scriptOptions = scriptOptions.AddImports(typeof(ScriptEvaluator<,>).Namespace);
                scriptOptions = scriptOptions.AddImports("Bogus");
                scriptOptions = scriptOptions.AddImports("Newtonsoft.Json.Linq");
                
                var script = CSharpScript.Create<TResult>(
                    _code,
                    globalsType: typeof(TContext),
                    options: scriptOptions);

                _runner = script.CreateDelegate();
            }
        }

        public async Task<TResult> Run(TContext context)
        {
            return await _runner(context);
        }
    }
}

