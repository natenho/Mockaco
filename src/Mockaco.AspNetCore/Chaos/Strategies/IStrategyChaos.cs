using Microsoft.AspNetCore.Http;

namespace Mockaco
{
    internal interface IStrategyChaos
    {
        Task Response(HttpResponse httpResponse);
    }
}
