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
        private readonly ILogger<NadraPayloadBuilder> _logger;

        public NadraPayloadBuilder(ILogger<NadraPayloadBuilder> logger)
        {
            _logger = logger;
        }
        public string Build(
                            string uid,
                            string msisdn,
                            int orderType,
                            dynamic citizen)
        {
            try
            {
                // Normalize MSISDN: 92xxxxxxxxxx → 03xxxxxxxxx
                string normalizedMsisdn = "0" + msisdn.Substring(2);

                var payload = new NadraRequest
                {
                    Data = new NadraData
                    {
                        Attributes = new NadraAttributes
                        {
                            Category = 1,
                            CitizenNumber = citizen.id_number,
                            CitizenType = citizen.id_type,
                            Msisdn = normalizedMsisdn,
                            RequestId = long.Parse(citizen.request_id),
                            SaleType = MapSaleType(orderType),
                            SessionId = citizen.session_id,
                            TransactionId = citizen.transaction_id,
                            ActivationDate = DateTime.Now.ToString("yyyy-MM-dd")
                        }
                    }
                };

                return JsonSerializer.Serialize(payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to build NADRA payload for UID {Uid}, MSISDN {Msisdn}",
                    uid,
                    msisdn);

                return ""; // Let caller decide how to mark FAILED
            }
        }

        private static int MapSaleType(int? orderType)
        {
            if (!orderType.HasValue)
            {
                return 1; // Default → New SIM
            }

            return orderType.Value switch
            {
                0 => 1, // New SIM / MNP
                10 => 3, // Change SIM / Duplicate SIM
                15 => 2, // Change Owner
                24 => 5, // Re-Verification
                29 => 6, // Disown
                _ =>  1 // Fallback → New SIM
            };
        }






    }
}
