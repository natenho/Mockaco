using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Mockaco.Chaos.Strategies;

internal class ChaosStrategyTimeout : IChaosStrategy
{
    private readonly IOptions<ChaosOptions> _options;

    public ChaosStrategyTimeout(IOptions<ChaosOptions> options)
    {
        _options = options;
    }
    
    public async Task Response(HttpResponse httpResponse)
    {
        await Task.Delay(_options.Value.TimeBeforeTimeout);

        httpResponse.StatusCode = (int)HttpStatusCode.RequestTimeout;

        var bodyBytes = Encoding.UTF8.GetBytes($"Error {httpResponse.StatusCode}: {HttpStatusCode.RequestTimeout}");

        await httpResponse.Body.WriteAsync(bodyBytes, 0, bodyBytes.Length, default);
    }
}