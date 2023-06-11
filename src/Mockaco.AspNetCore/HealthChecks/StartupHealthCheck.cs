using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Mockaco.HealthChecks
{
    public class StartupHealthCheck : IHealthCheck
    {
        private volatile bool _isReady;

        public bool StartupCompleted
        {
            get => _isReady;
            set => _isReady = value;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (StartupCompleted)
            {
                return Task.FromResult(HealthCheckResult.Healthy("The startup has completed."));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy("That startup is still running."));
        }
    }
}
