using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;

namespace Mockaco
{
    internal class StrategyChaosTimeout : IStrategyChaos
    {
        private readonly IOptions<ChaosOptions> _options;

        public StrategyChaosTimeout(IOptions<ChaosOptions> options) {
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
}
