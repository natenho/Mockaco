using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Mockaco
{
    public interface ITemplateResponseProcessor
    {
        Task PrepareResponse(HttpResponse httpResponse, IScriptContext scriptContext, ResponseTemplate template);
    }
}