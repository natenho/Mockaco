using Microsoft.Extensions.Hosting;

namespace Mockaco
{
    internal sealed class MockProviderWarmUp : IHostedService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IMockProvider _mockProvider;

        public MockProviderWarmUp(IHostApplicationLifetime hostApplicationLifetime, IMockProvider mockProvider)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _mockProvider = mockProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _hostApplicationLifetime.ApplicationStarted.Register(OnStarted);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private void OnStarted()
        {
            _mockProvider.WarmUp();
        }
    }
}
