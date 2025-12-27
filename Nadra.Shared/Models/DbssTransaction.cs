using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadra.Shared.Models
{
    public sealed class DbssTransaction
    {
        public string UID { get; init; }
        public string MSISDN { get; init; }
        public string ICC { get; init; }
        public string SHOP_ID { get; init; }
        public string ORDER_ID { get; init; }
        public string DBSSUSERNAME { get; init; }
        public string DEALER_CODE { get; init; }
        public string ORDER_TYPE { get; init; }
        public string ORDER_TYPE_VALUE { get; init; }
        public decimal? PAID_AMOUNT { get; init; }
        public DateTime? INSERT_DATE { get; init; }
    }
}
