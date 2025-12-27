using Microsoft.Extensions.Options;
using Nadra.Picker.Worker.Options;
using Nadra.Picker.Worker.Repositories;

namespace Nadra.Picker.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly PickerRepository _repo;
        private readonly PickerOptions _options;

        public Worker(ILogger<Worker> logger,
                      PickerRepository repo,
                      IOptions<PickerOptions> options)
        {
            _logger = logger;
            _repo = repo;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var picked = await _repo.PickAsync(
                    _options.BatchSize,
                    stoppingToken);

                if (picked == 0)
                {
                    await Task.Delay(
                        _options.PollDelayMs,
                        stoppingToken);
                }
            }
        }
    }
}
