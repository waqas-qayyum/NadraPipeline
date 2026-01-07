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
                    _logger.LogInformation("Fetching PICKED records start");

                    var picked = await _tracker.FetchPickedAsync(
                    _options.BatchSize);

                    var any = false;


                    _logger.LogInformation(
                        "Fetched {Count} PICKED records",
                        picked.Count());

                    if (picked.Count() == 0)
                    {
                        _logger.LogInformation("No records found, entering idle delay");
                        await Task.Delay(_options.PollDelayMs, stoppingToken);
                        continue;
                    }

                    foreach (var record in picked)
                    {
                        any = true;

                        var citizen =
                            await _citizenRepo.GetCitizenAsync(
                                record.MSISDN);

                        if (citizen == null)
                        {
                            _logger.LogWarning(
                                "No citizen data found for UID {Uid}, MSISDN {Msisdn}",
                                record.UID, record.MSISDN);

                            await _tracker.MarkFailedAsync(
                                record.UID,
                                "Citizen data not found");

                            continue;
                        }                       

                        var payload = _builder.Build(
                            record.UID,
                            record.MSISDN,
                            Convert.ToInt32(record.OrderType),
                            citizen);

                        if (string.IsNullOrWhiteSpace(payload))
                        {
                            _logger.LogWarning(
                                "Failed to build payload for UID {Uid}, MSISDN {Msisdn}",
                                record.UID, record.MSISDN);
                            await _tracker.MarkFailedAsync(
                                record.UID,
                                "Failed to build payload");
                            continue;
                        }

                        // ---- Success Path ----
                        await _tracker.MarkEnrichedAsync(
                            record.UID,
                            payload);
                    }

                    if (!any)
                    {
                        _logger.LogInformation("No record in Any");

                        await Task.Delay(
                            _options.PollDelayMs,
                            stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Worker execution failed");
                }

            }
        }
    }
}
