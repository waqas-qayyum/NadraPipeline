using Microsoft.Extensions.Options;
using Nadra.Enrichment.Worker.Options;
using Nadra.Enrichment.Worker.Repositories;
using Nadra.Enrichment.Worker.Services;

namespace Nadra.Enrichment.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly TrackerRepository _tracker;
        private readonly CitizenLookupRepository _citizenRepo;
        private readonly NadraPayloadBuilder _builder;
        private readonly EnrichmentOptions _options;

        public Worker(ILogger<Worker> logger,
                        TrackerRepository tracker,
                        CitizenLookupRepository citizenRepo,
                        NadraPayloadBuilder builder,
                        IOptions<EnrichmentOptions> options)
        {
            _logger = logger;
            _tracker = tracker;
            _citizenRepo = citizenRepo;
            _builder = builder;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker started");

            // Let the service start first
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var picked = await _tracker.FetchPickedAsync(
                    _options.BatchSize);

                    var any = false;

                    foreach (var record in picked)
                    {
                        any = true;

                        var citizen =
                            await _citizenRepo.GetCitizenAsync(
                                record.MSISDN);

                        if (citizen == null)
                            continue; // optionally mark FAILED

                        var payload = _builder.Build(
                            record.UID,
                            record.MSISDN,
                            Convert.ToInt32(record.OrderType),
                            citizen);

                        await _tracker.MarkEnrichedAsync(
                            record.UID,
                            payload);
                    }

                    if (!any)
                        await Task.Delay(
                            _options.PollDelayMs,
                            stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Worker execution failed");
                }

            }
        }
    }
}
