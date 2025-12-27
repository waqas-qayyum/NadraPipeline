using Microsoft.Extensions.Options;
using Nadra.Dispatcher.Worker.Options;
using Nadra.Dispatcher.Worker.Services;

namespace Nadra.Dispatcher.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly DispatchCoordinator _coordinator;
        private readonly DispatcherOptions _options;

        public Worker(ILogger<Worker> logger,
                      DispatchCoordinator coordinator,
                      IOptions<DispatcherOptions> options)
        {
            _logger = logger;
            _coordinator = coordinator;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }

                await _coordinator.DispatchAsync(stoppingToken);
                await Task.Delay(
                    _options.PollDelayMs,
                    stoppingToken);
                
            }
        }
    }
}
