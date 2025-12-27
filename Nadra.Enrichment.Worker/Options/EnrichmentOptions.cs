using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadra.Enrichment.Worker.Options
{
    public sealed class EnrichmentOptions
    {
        public int BatchSize { get; init; }
        public int PollDelayMs { get; init; }
    }
}
