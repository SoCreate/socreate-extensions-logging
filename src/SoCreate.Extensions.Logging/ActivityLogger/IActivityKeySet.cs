using System.Collections.Generic;

namespace SoCreate.Extensions.Logging.ActivityLogger
{
    public interface IActivityKeySet
    {
        Dictionary<string, string> ToDictionary();
    }
}