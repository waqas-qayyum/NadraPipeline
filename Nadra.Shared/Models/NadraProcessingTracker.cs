using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadra.Shared.Models
{
    public sealed class NadraProcessingTracker
    {
        public string UID { get; init; }          // maps to DBSS UID
        public string MSISDN { get; init; }
        public string OrderType { get; init; }
        public string Status { get; set; }
        public int AttemptCount { get; set; }
        public DateTime PickedAt { get; init; }
    }
}
