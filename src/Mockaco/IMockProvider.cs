using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mockaco.Routing
{
    public interface IMockProvider
    {
        List<Mock> GetMocks();

        IEnumerable<(string TemplateName, string ErrorMessage)> GetErrors();

        Task WarmUp();
    }
}