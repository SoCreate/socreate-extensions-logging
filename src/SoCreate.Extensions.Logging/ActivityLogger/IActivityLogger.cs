using System.Collections.Generic;

namespace SoCreate.Extensions.Logging.ActivityLogger
{
    public interface IActivityLogger
    {
        void LogActivity<TActivityEnum>(IActivityKeySet keySet, TActivityEnum actionType,
            Dictionary<string, object> additionalData, string message,
            params object[] messageData);
    }
    
    public interface IActivityLogger<out TSourceContext>: IActivityLogger
    {
    }
}