using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Mockaco
{
    public interface ITemplateProcessor
    {
        Task ProcessResponse(HttpContext httpContext);
    }
}