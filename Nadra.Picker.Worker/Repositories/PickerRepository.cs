using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadra.Picker.Worker.Repositories
{
    public sealed class PickerRepository
    {
        private readonly string _connectionString;

        public PickerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }


        /// <summary>
        /// Order_Type	Transaction_Type
        //  NewSim			0
        //  MNP             0 - need to check 2 tables [CYN and CD Pack if not found then consider this as MNP]
        //  Change SIM		10
        //  Change Owner	15
        //  Re-verification	24
        //  Disown      	29
        /// </summary>
        /// <param name="batchSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<int> PickAsync(int batchSize, CancellationToken ct)
        {
            const string sql = @"
        ;WITH CTE_PICK AS (
            SELECT TOP (@BatchSize)
                t.UID,
                t.MSISDN,
                t.TRANSACTION_TYPE as ORDER_TYPE
            FROM dbo.DBSS_ALL_TRANSACTION_DATA t
            CROSS JOIN dbo.NADRA_PROCESSING_CONFIG c
            LEFT JOIN dbo.NADRA_PROCESSING_TRACKER p
                ON p.UID = t.UID
            WHERE
                t.TRANSACTION_TYPE IN (0, 10, 15, 24, 29)
                AND t.UID > c.UIC_CUT_OFF
                AND p.UID IS NULL
            ORDER BY t.INSERT_DATE
        )
        INSERT INTO dbo.NADRA_PROCESSING_TRACKER (
            UID, MSISDN, ORDER_TYPE, STATUS
        )
        SELECT UID, MSISDN, ORDER_TYPE, 'PICKED'
        FROM CTE_PICK;
        ";

            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteAsync(sql, new { BatchSize = batchSize });
        }
    }
}
