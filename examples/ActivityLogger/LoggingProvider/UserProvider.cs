using SoCreate.Extensions.Logging.ActivityLogger.LoggingProvider;

namespace ActivityLogger.LoggingProvider
{
    public class ProfileProvider : IProfileProvider
    {
        public int GetProfileId()
        {
            // Hardcoded as an example
            return 1;
        }
    }
}