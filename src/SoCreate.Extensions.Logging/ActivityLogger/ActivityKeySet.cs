using System.Collections.Generic;
using System.Linq;

namespace SoCreate.Extensions.Logging.ActivityLogger
{
    public abstract class ActivityKeySet : IActivityKeySet
    {
        protected static Dictionary<string, string> MemoisedDictionary;

        public virtual Dictionary<string, string> ToDictionary()
        {
            if (MemoisedDictionary != null) return MemoisedDictionary;

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

            MemoisedDictionary = keyDictionary;
            return keyDictionary;
        }
    }
}