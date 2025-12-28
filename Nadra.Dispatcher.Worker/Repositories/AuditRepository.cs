using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadra.Dispatcher.Worker.Repositories
{
    public sealed class AuditRepository
    {
        private readonly string _connectionString;

        public AuditRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DbssDbProd")
                ?? throw new InvalidOperationException(
                    "Connection string 'DbssDbProd' not found");
        }


        public async Task InsertAsync(
            string uid,
            string request,
            string? response,
            int? httpStatus,
            string? responseCode)
        {
            const string sql = @"
        INSERT INTO dbo.NADRA_DISPATCH_AUDIT (
            UID,
            REQUEST_PAYLOAD,
            RESPONSE_PAYLOAD,
            RESPONSE_CODE,
            HTTP_STATUS
        )
        VALUES (
            @Uid,
            @Request,
            @Response,
            @ResponseCode,
            @HttpStatus
        );
        ";

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new
            {
                Uid = uid,
                Request = request,
                Response = response,
                ResponseCode = responseCode,
                HttpStatus = httpStatus
            });
        }
    }

}
