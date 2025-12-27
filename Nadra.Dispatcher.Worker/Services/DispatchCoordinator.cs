using Microsoft.Extensions.Options;
using Nadra.Dispatcher.Worker.Options;
using Nadra.Dispatcher.Worker.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadra.Dispatcher.Worker.Services
{
    public sealed class DispatchCoordinator
    {
        private readonly TrackerRepository _tracker;
        private readonly AuditRepository _audit;
        private readonly NadraApiClient _client;
        private readonly DispatcherOptions _options;

        public DispatchCoordinator(
            TrackerRepository tracker,
            AuditRepository audit,
            NadraApiClient client,
            IOptions<DispatcherOptions> options)
        {
            _tracker = tracker;
            _audit = audit;
            _client = client;
            _options = options.Value;
        }

        public async Task DispatchAsync(
            CancellationToken ct)
        {
            var records =
                await _tracker.FetchEnrichedAsync(
                    _options.BatchSize);

            foreach (var (uid, payload, attempts) in records)
            {
                try
                {
                    var response =
                        await _client.SendAsync(payload, ct);

                    var body =
                        await response.Content.ReadAsStringAsync(ct);

                    await _audit.InsertAsync(
                        uid,
                        payload,
                        body,
                        (int)response.StatusCode,
                        response.IsSuccessStatusCode ? "SUCCESS" : "ERROR");

                    if (response.IsSuccessStatusCode)
                    {
                        await _tracker.MarkSentAsync(uid);
                    }
                    else
                    {
                        await HandleRetry(uid, body, attempts);
                    }
                }
                catch (Exception ex)
                {
                    await HandleRetry(uid, ex.Message, attempts);
                }
            }
        }

        private async Task HandleRetry(
            string uid,
            string error,
            int attempts)
        {
            if (attempts + 1 >= _options.MaxRetries)
            {
                await _tracker.MarkFailedAsync(uid, error);
            }
            else
            {
                await _tracker.IncrementAttemptAsync(uid);
            }
        }
    }
}
