using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mockaco.Chaos.Strategies;

internal class ChaosStrategyLatency : IChaosStrategy
{
    private readonly ILogger<ChaosStrategyLatency> _logger;
    private readonly IOptions<ChaosOptions> _options;

    public ChaosStrategyLatency(ILogger<ChaosStrategyLatency> logger, IOptions<ChaosOptions> options)
    {
        _logger = logger;
        _options = options;
    }
    public Task Response(HttpResponse httpResponse)
    {
        var responseDelay = new Random().Next(_options.Value.MinimumLatencyTime, _options.Value.MaximumLatencyTime);
        _logger.LogInformation($"Chaos Latency (ms): {responseDelay}");
        return Task.Delay(responseDelay);
    }
}