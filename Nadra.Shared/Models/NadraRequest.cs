using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nadra.Shared.Models
{
    public sealed class NadraRequest
    {
        [JsonPropertyName("data")]
        public NadraData Data { get; init; }
    }

    public sealed class NadraData
    {
        [JsonPropertyName("type")]
        public string Type { get; init; } = "biometrics";

        [JsonPropertyName("attributes")]
        public NadraAttributes Attributes { get; init; }
    }

    public sealed class NadraAttributes
    {
        [JsonPropertyName("category")]
        public int Category { get; init; }

        [JsonPropertyName("citizen-number")]
        public string CitizenNumber { get; init; }

        [JsonPropertyName("citizen-type")]
        public int CitizenType { get; init; }   // NOTE: numeric in required JSON

        [JsonPropertyName("msisdn")]
        public string Msisdn { get; init; }

        [JsonPropertyName("request-id")]
        public long RequestId { get; init; }    // NOTE: numeric in required JSON

        [JsonPropertyName("sale-type")]
        public int SaleType { get; init; }

        [JsonPropertyName("session-id")]
        public string SessionId { get; init; }

        [JsonPropertyName("transaction-id")]
        public string TransactionId { get; init; }

        [JsonPropertyName("activation-date")]
        public string ActivationDate { get; init; }
    }
}
