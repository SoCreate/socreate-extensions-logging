using System.Collections.Generic;

namespace SoCreate.Extensions.Logging.ActivityLogger
{
    public interface IActivityLogger
    {
        void LogActivity<TActivityEnum>(IActivityKeySet keySet, TActivityEnum actionType, string message,
            params object[] messageData);
        
        void LogActivity<TActivityEnum>(IActivityKeySet keySet, TActivityEnum actionType,
            AdditionalData additionalData, string message,
            params object[] messageData);
    }
    
    public interface IActivityLogger<out TSourceContext>: IActivityLogger
    {
    }
}