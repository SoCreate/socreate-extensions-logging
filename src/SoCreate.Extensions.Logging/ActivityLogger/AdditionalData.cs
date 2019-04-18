using System;
using System.Collections.Generic;
using System.Linq;
using Serilog.Events;

namespace SoCreate.Extensions.Logging.ActivityLogger
{
    public class AdditionalData
    {
        public Dictionary<string, object> Properties { get; set; }

        public AdditionalData(params ValueTuple<string, object>[] pairs)
        {
            Properties = pairs.ToDictionary(x => x.Item1, x => x.Item2);
        }
    }
}