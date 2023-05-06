using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;

namespace Mockaco
{
    internal class StrategyChaosLatency : IStrategyChaos
    {
        private readonly ILogger<StrategyChaosLatency> _logger;
        private readonly IOptions<ChaosOptions> _options;

        public StrategyChaosLatency(ILogger<StrategyChaosLatency> logger,
            IOptions<ChaosOptions> options)
        {
            _logger = logger;
            _options = options;
        }
        public async Task Response(HttpResponse httpResponse)
        {
            var responseDelay = new Random().Next(_options.Value.MinimumLatencyTime, _options.Value.MaximumLatencyTime);
            _logger.LogInformation($"Chaos Latency (ms): {responseDelay}");
            await Task.Delay(responseDelay);
        }
    }
}
