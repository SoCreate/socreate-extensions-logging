using SoCreate.Extensions.Logging.ActivityLogger;

namespace ActivityLogger
{
    public class ExampleKeySet : ActivityKeySet
    {
        public const string SpecialExampleIdKey = "SpecialExampleId";
        public int SpecialExampleId { get; set; }
    }
}