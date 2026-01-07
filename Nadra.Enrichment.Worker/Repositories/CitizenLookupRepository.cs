using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;


namespace Nadra.Enrichment.Worker.Repositories
{
    public sealed class CitizenLookupRepository
    {
        private readonly string _connectionString;

        public CitizenLookupRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<dynamic?> GetCitizenAsync(string msisdn)
        {
            try
            {
                const string sql = @"
                        SELECT
                            session_id,
                            transaction_id,
                            id_number,
                            request_id,
                            order_type,
                            CASE id_type
                                WHEN 'national_id_fingerprint' THEN 1
                                WHEN 'proof_of_registration'   THEN 2
                                WHEN 'pakistan_origin_card'    THEN 3
                                WHEN 'passport'                THEN 4
                                ELSE NULL
                            END AS id_type,
                            SALE_TYPE,
                            bv_timestamp
                        FROM (
                            SELECT
                                session_id,
                                transaction_id,
                                request_id,
                                order_type,
                                id_type,
                                id_number,
                                1 AS SALE_TYPE,
                                bv_timestamp,
                                ROW_NUMBER() OVER (
                                    ORDER BY
                                        bv_timestamp DESC,
                                        transaction_id DESC
                                ) AS rn
                            FROM biometric_nadratransaction
                            WHERE bv_status = 'verified'
                              AND msisdn = @Msisdn
                        ) t
                        WHERE rn = 1;
        ";

                using var conn = new MySqlConnection(_connectionString);
                return await conn.QuerySingleOrDefaultAsync(
                    sql, new { Msisdn = msisdn });
            }
            catch (Exception)
            {
                // Log exception if logging is set up
                return null;
            }
        }
    }
}
