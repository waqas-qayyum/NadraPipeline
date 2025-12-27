using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadra.Picker.Worker.Options
{
    public sealed class PickerOptions
    {
        public int BatchSize { get; init; }
        public int PollDelayMs { get; init; }
    }
}
