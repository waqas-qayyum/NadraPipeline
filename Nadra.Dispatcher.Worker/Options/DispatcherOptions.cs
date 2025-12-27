using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadra.Dispatcher.Worker.Options
{
    public sealed class DispatcherOptions
    {
        public int BatchSize { get; init; }
        public int PollDelayMs { get; init; }
        public int MaxRetries { get; init; }
    }
}
