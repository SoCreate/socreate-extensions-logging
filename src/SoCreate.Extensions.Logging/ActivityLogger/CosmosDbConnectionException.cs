using System;

namespace SoCreate.Extensions.Logging.ActivityLogger
{
    public class CosmosDbConnectionException : Exception
    {
        public CosmosDbConnectionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}