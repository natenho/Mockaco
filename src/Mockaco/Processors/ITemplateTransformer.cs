using System.Threading.Tasks;

namespace Mockaco.Processors
{
    public interface ITemplateTransformer
    {
        Task<string> Transform(string input, ScriptContext scriptContext);
    }
}