using System;

namespace SoCreate.Extensions.Logging
{
    public class ServiceFabricLoggerOptions
    {
        public string ServiceName { get; set; }
        public string ServiceTypeName { get; set; }
        public bool UseApplicationInsights { get; set; }
        // This is used for getting the user id when application insights is active
        public Func<int> GetUserIdFromContext { get; set; }
        public bool UseActivityLogger { get; set; }
        
    }
}