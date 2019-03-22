namespace SoCreate.Extensions.Logging
{
    public class ServiceFabricLoggerOptions
    {
        public string ServiceName { get; set; }
        public string ServiceTypeName { get; set; }
        public bool UseApplicationInsights { get; set; }
        public bool UseActivityLogger { get; set; }
    }
}