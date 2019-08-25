using System.Threading.Tasks;

namespace Mockaco
{
    public interface ITemplateTransformer
    {
        Task<Template> Transform(IRawTemplate rawTemplate, IScriptContext scriptContext);
    }
}