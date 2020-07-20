using System.Threading.Tasks;

namespace Mockaco
{
    public interface ITemplateTransformer
    {
        //TODO Improve this abstraction
        Task<Template> TransformAndSetVariables(IRawTemplate rawTemplate, IScriptContext scriptContext);

        Task<Template> Transform(IRawTemplate rawTemplate, IScriptContext scriptContext);
    }
}