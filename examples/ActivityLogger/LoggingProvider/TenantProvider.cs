using SoCreate.Extensions.Logging.ActivityLogger.LoggingProvider;

namespace ActivityLogger.LoggingProvider;

public class TenantProvider : ITenantProvider
{
    public int GetTenantId()
    {
        // hardcoded as an example
        return 100;
    }
}