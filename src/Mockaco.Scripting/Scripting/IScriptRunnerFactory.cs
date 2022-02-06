using Microsoft.CodeAnalysis.Scripting;
using System.Threading.Tasks;

namespace Mockaco
{
    public interface IScriptRunnerFactory
    {
        ScriptRunner<TResult> CreateRunner<TContext, TResult>(string code);

        Task<TResult> Invoke<TContext, TResult>(TContext context, string code);
    }
}