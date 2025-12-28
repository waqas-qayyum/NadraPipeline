using Dapper;
using Microsoft.Data.SqlClient;
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

        public TrackerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<NadraProcessingTracker>> FetchPickedAsync(
            int batchSize)
        {
            const string sql = @"
        SELECT TOP (@BatchSize)
            UID,
            MSISDN,
            ORDER_TYPE  AS OrderType
        FROM dbo.NADRA_PROCESSING_TRACKER
        WHERE STATUS = 'PICKED'
        ORDER BY PICKED_AT;
        ";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<NadraProcessingTracker>(
                sql, new { BatchSize = batchSize });
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

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { Uid = uid, Payload = payload });
        }
    }
}
