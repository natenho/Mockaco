using System.Threading.Tasks;

namespace Mockaco.Processors
{
    public interface ITemplateTransformer
    {
        Task<Template> Transform(IRawTemplate rawTemplate, IScriptContext scriptContext);
    }
}