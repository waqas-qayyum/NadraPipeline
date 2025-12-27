using Nadra.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nadra.Enrichment.Worker.Services
{
    public sealed class NadraPayloadBuilder
    {
        public string Build(
            string uid,
            string msisdn,
            string orderType,
            dynamic citizen)
        {
            var payload = new NadraRequest
            {
                Data = new NadraData
                {
                    Attributes = new NadraAttributes
                    {
                        Category = 1,
                        CitizenNumber = citizen.id_number,
                        CitizenType = citizen.id_type,
                        Msisdn = msisdn,
                        RequestId = long.Parse(citizen.request_id),
                        SaleType = citizen.SALE_TYPE,
                        SessionId = citizen.session_id,
                        TransactionId = citizen.transaction_id,
                        // citizen.bv_timestamp
                        ActivationDate = DateTime.Now.ToString("yyyy-MM-dd")
                    }
                }
            };

            return JsonSerializer.Serialize(payload);
        }
    }
}
