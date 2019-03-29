using System;
using System.Collections.Generic;
using System.Linq;

namespace SoCreate.Extensions.Logging.ActivityLogger
{
    public class AdditionalData
    {
        public Dictionary<string, object> Dictionary { get; set; }

        public AdditionalData(params ValueTuple<string, object>[] pairs)
        {
            Dictionary = pairs.ToDictionary(x => x.Item1, x => x.Item2);
        }
    }
}