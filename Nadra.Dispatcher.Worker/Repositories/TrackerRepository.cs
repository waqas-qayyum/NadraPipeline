using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadra.Dispatcher.Worker.Repositories
{
    public sealed class TrackerRepository
    {
        private readonly string _connectionString;

        public TrackerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<(string Uid, string Payload, int Attempts)>>
            FetchEnrichedAsync(int batchSize)
        {
            const string sql = @"
        SELECT TOP (@BatchSize)
            UID,
            NADRA_PAYLOAD,
            ATTEMPT_COUNT
        FROM dbo.NADRA_PROCESSING_TRACKER
        WHERE STATUS = 'ENRICHED'
        ORDER BY LAST_UPDATED_AT;
        ";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<(string, string, int)>(
                sql, new { BatchSize = batchSize });
        }

        public async Task MarkSentAsync(string uid)
        {
            const string sql = @"
        UPDATE dbo.NADRA_PROCESSING_TRACKER
        SET STATUS = 'SENT',
            LAST_UPDATED_AT = SYSDATETIME()
        WHERE UID = @Uid;
        ";

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { Uid = uid });
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

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { Uid = uid, Error = error });
        }

        public async Task IncrementAttemptAsync(string uid)
        {
            const string sql = @"
        UPDATE dbo.NADRA_PROCESSING_TRACKER
        SET ATTEMPT_COUNT = ATTEMPT_COUNT + 1
        WHERE UID = @Uid;
        ";

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { Uid = uid });
        }
    }
}
