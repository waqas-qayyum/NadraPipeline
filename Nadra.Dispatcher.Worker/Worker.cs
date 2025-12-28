using Microsoft.Extensions.Options;
using Nadra.Dispatcher.Worker.Options;
using Nadra.Dispatcher.Worker.Services;

namespace Nadra.Dispatcher.Worker
{
    public sealed class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DispatcherOptions _options;

        public Worker(
            ILogger<Worker> logger,
            IServiceScopeFactory scopeFactory,
            IOptions<DispatcherOptions> options)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation("Nadra Dispatcher Worker started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var coordinator =
                        scope.ServiceProvider
                             .GetRequiredService<DispatchCoordinator>();

                    await coordinator.DispatchAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Normal shutdown
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Unhandled exception in dispatcher loop");
                }

                await Task.Delay(
                    _options.PollDelayMs,
                    stoppingToken);
            }
        }
    }

}
