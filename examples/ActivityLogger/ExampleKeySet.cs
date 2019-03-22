using System.Collections.Generic;
using System.Linq;
using SoCreate.Extensions.Logging.ActivityLogger;

namespace ActivityLogger
{
    public class ExampleKeySet : IActivityKeySet
    {
        public const string SpecialExampleIdKey = "SpecialExampleId";

        public int SpecialExampleId { get; set; }

        public Dictionary<string, string> ToDictionary()
        {
            var keyDictionary = new Dictionary<string, string>();
            var fields = GetType().GetFields();

            foreach (var prop in GetType().GetProperties())
            {
                if (prop.GetValue(this) != null)
                {
                    var keyName = fields.First(f => f.Name == prop.Name + "Key");
                    keyDictionary.Add(keyName.GetValue(this).ToString(), prop.GetValue(this).ToString());
                }
            }

            return keyDictionary;
        }
    }
}