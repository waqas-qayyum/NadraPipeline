using Dapper;
using Microsoft.Data.SqlClient;
using Nadra.Enrichment.Worker.Services;
using Nadra.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadra.Enrichment.Worker.Repositories
{
    public sealed class TrackerRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<TrackerRepository> _logger;

        public TrackerRepository(ILogger<TrackerRepository> logger,
                                 string connectionString)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task<IEnumerable<NadraProcessingTracker>> FetchPickedAsync(
            int batchSize)
        {
            const string sql = @"
                    SELECT TOP (@BatchSize)
                        UID,
                        MSISDN,
                        ORDER_TYPE AS OrderType
                    FROM dbo.NADRA_PROCESSING_TRACKER WITH (ROWLOCK, READPAST)
                    WHERE
                        STATUS = 'PICKED'
                        AND MSISDN IS NOT NULL
                        AND LTRIM(RTRIM(MSISDN)) <> ''
                    ORDER BY PICKED_AT;
                ";

            try
            {
                using var conn = new SqlConnection(_connectionString);

                return await conn.QueryAsync<NadraProcessingTracker>(
                    sql,
                    new { BatchSize = batchSize });
            }
            catch (OperationCanceledException)
            {
                // Respect cancellation — let worker exit cleanly
                throw;
            }
            catch (SqlException ex)
            {
                // DB-specific issue (deadlock, timeout, connectivity)
                _logger.LogError(
                    ex,
                    "Database error in FetchPickedAsync. Returning empty result set.");

                return Enumerable.Empty<NadraProcessingTracker>();
            }
            catch (Exception ex)
            {
                // Absolute last safety net
                _logger.LogError(
                    ex,
                    "Unexpected error in FetchPickedAsync. Returning empty result set.");

                return Enumerable.Empty<NadraProcessingTracker>();
            }
        }

        public async Task MarkEnrichedAsync(
    string uid,
    string payload)
        {
            const string sql = @"
                        UPDATE dbo.NADRA_PROCESSING_TRACKER
                        SET STATUS = 'ENRICHED',
                            NADRA_PAYLOAD = @Payload,
                            LAST_UPDATED_AT = SYSDATETIME()
                        WHERE UID = @Uid;
                    ";

            try
            {
                using var conn = new SqlConnection(_connectionString);

                var rows = await conn.ExecuteAsync(
                    sql,
                    new { Uid = uid, Payload = payload },
                    commandTimeout: 10);

                if (rows == 0)
                {
                    // Defensive: UID vanished or already updated
                    _logger.LogWarning(
                        "MarkEnrichedAsync affected 0 rows for UID {Uid}",
                        uid);
                }
            }
            catch (OperationCanceledException)
            {
                // Respect worker shutdown
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(
                    ex,
                    "Database error while marking ENRICHED for UID {Uid}",
                    uid);

                // Optional: downgrade to FAILED
                await SafeMarkFailedAsync(
                    uid,
                    "DB error while marking ENRICHED");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while marking ENRICHED for UID {Uid}",
                    uid);

                // Optional: downgrade to FAILED
                await SafeMarkFailedAsync(
                    uid,
                    "Unexpected error while marking ENRICHED");
            }
        }


        public async Task MarkFailedAsync(
                                        string uid,
                                        string error)
        {
            const string sql = @"
                            UPDATE dbo.NADRA_PROCESSING_TRACKER
                            SET STATUS = 'FAILED',
                                LAST_ERROR = @Error,
                                LAST_UPDATED_AT = SYSDATETIME()
                            WHERE UID = @Uid;
                        ";

            try
            {
                using var conn = new SqlConnection(_connectionString);

                var rows = await conn.ExecuteAsync(
                    sql,
                    new
                    {
                        Uid = uid,
                        Error = Truncate(error, 1000) // safety against SQL truncation
                    },
                    commandTimeout: 10);

                if (rows == 0)
                {
                    _logger.LogWarning(
                        "MarkFailedAsync affected 0 rows for UID {Uid}",
                        uid);
                }
            }
            catch (OperationCanceledException)
            {
                // Respect shutdown — let worker stop cleanly
                throw;
            }
            catch (SqlException ex)
            {
                // NEVER throw from here
                _logger.LogError(
                    ex,
                    "Database error while marking FAILED for UID {Uid}",
                    uid);
            }
            catch (Exception ex)
            {
                // Absolute last line of defense
                _logger.LogCritical(
                    ex,
                    "Unexpected error while marking FAILED for UID {Uid}",
                    uid);
            }
        }

        private async Task SafeMarkFailedAsync(
                                                string uid,
                                                string reason)
        {
            try
            {
                const string sql = @"
            UPDATE dbo.NADRA_PROCESSING_TRACKER
            SET STATUS = 'FAILED',
                FAILURE_REASON = @Reason,
                LAST_UPDATED_AT = SYSDATETIME()
            WHERE UID = @Uid;
        ";

                using var conn = new SqlConnection(_connectionString);
                await conn.ExecuteAsync(
                    sql,
                    new { Uid = uid, Reason = reason },
                    commandTimeout: 10);
            }
            catch (Exception ex)
            {
                // FINAL SAFETY NET — NEVER THROW
                _logger.LogCritical(
                    ex,
                    "CRITICAL: Unable to mark FAILED for UID {Uid}",
                    uid);
            }
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            return value.Length <= maxLength
                ? value
                : value.Substring(0, maxLength);
        }
    }
}
