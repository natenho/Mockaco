using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Mockore
{
    public interface ITemplateProcessor
    {
        Task ProcessResponse(HttpContext httpContext);
    }
}