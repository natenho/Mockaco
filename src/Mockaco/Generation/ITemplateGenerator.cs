using System.Threading.Tasks;

namespace Mockaco.Generation
{
    public interface ITemplateGenerator
    {
        Task GenerateTemplateAsync(TemplateGenerationContext context);
    }
}
