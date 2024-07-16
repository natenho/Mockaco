using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Mockaco.Chaos.Strategies;

internal class ChaosStrategyException : IChaosStrategy
{
    public Task Response(HttpResponse httpResponse)
    {
        httpResponse.StatusCode = (int)HttpStatusCode.InternalServerError;

        var bodyBytes = Encoding.UTF8.GetBytes($"Error {httpResponse.StatusCode}: {HttpStatusCode.ServiceUnavailable}");

        return httpResponse.Body.WriteAsync(bodyBytes, 0, bodyBytes.Length);
    }
}