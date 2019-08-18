using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mockaco.Routing
{
    public interface IRouteProvider
    {
        List<Route> GetRoutes();

        IEnumerable<(string TemplateName, string ErrorMessage)> GetErrors();

        Task WarmUp();
    }
}