using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;

namespace Mockaco
{
    internal class StrategyChaosBehavior : IStrategyChaos
    {
        public async Task Response(HttpResponse httpResponse)
        {
            httpResponse.StatusCode = (int)HttpStatusCode.ServiceUnavailable;

            var bodyBytes = Encoding.UTF8.GetBytes($"Error {httpResponse.StatusCode}: {HttpStatusCode.ServiceUnavailable}");

            await httpResponse.Body.WriteAsync(bodyBytes, 0, bodyBytes.Length, default);
        }
    }
}
