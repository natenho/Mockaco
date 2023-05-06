using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;

namespace Mockaco
{
    internal class StrategyChaosException : IStrategyChaos
    {
        public async Task Response(HttpResponse httpResponse)
        {
            httpResponse.StatusCode = (int)HttpStatusCode.InternalServerError;

            var bodyBytes = Encoding.UTF8.GetBytes($"Error {httpResponse.StatusCode}: {HttpStatusCode.InternalServerError}");

            await httpResponse.Body.WriteAsync(bodyBytes, 0, bodyBytes.Length, default);
        }
    }
}
