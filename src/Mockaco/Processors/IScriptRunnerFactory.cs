using Microsoft.CodeAnalysis.Scripting;
using System.Threading.Tasks;

namespace Mockore
{
    public interface IScriptRunnerFactory
    {
        ScriptRunner<TResult> CreateRunner<TContext, TResult>(string code, out ScriptRunner<TResult> runner);
        Task<TResult> Invoke<TContext, TResult>(TContext context, string code);
    }
}