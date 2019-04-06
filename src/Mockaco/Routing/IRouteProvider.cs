using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mockaco.Routing
{
    public interface IRouteProvider
    {
        List<Route> GetRoutes();
        Task WarmUp();
    }
}