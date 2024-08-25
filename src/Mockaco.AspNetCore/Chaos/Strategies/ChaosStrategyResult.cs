using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Mockaco.Chaos.Strategies;

internal class ChaosStrategyResult : IChaosStrategy
{
    public Task Response(HttpResponse httpResponse)
    {
        httpResponse.StatusCode = (int)HttpStatusCode.BadRequest;

        var bodyBytes = Encoding.UTF8.GetBytes($"Error {httpResponse.StatusCode}: {HttpStatusCode.BadRequest}");

        return httpResponse.Body.WriteAsync(bodyBytes, 0, bodyBytes.Length);
    }
}