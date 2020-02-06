# SoCreate Logging

This project is used to add both application insights and SqlServer logging through serilog.

## Add Application Insights Logging
Add the instrument key to the appSettings.json.
```json
{
    "ApplicationInsights" : {
        "InstrumentationKey" : "<Key>"
    }
}
```
Set the SendLogDataToApplicationInsights Option to true. You can optionally add the user provider that will set the relevant user id on each log.
```c#
var host = new HostBuilder()
    .ConfigureLogging((hostingContext, builder) => 
         builder.AddServiceLogging(
            new ServiceLoggingConfiguration(hostingContext, new LoggerOptions { SendLogDataToApplicationInsights = true})
                .WithUserProvider(typeof(UserProvider))
        );
)
    .Build();

```


## Add Activity Logging (Powered By SqlServer)

Set the SendLogActivityDataToSql Option to true. When setting the SendLogActivityDataToSql, you must send in the keytype as well as adding 
the providers for user, account and tenant. The example project has examples of implemented classes for those interfaces.
```c#
var host = new HostBuilder()
    .ConfigureLogging((hostingContext, builder) => 
        builder.AddServiceLogging<ExampleKeyTypeEnum>(
            new ServiceLoggingConfiguration(hostingContext, new LoggerOptions { SendLogActivityDataToSql = true })
                .WithUserProvider(typeof(UserProvider))
                .WithAccountProvider<ExampleKeyTypeEnum>(typeof(AccountProvider))
                .WithTenantProvider(typeof(TenantProvider))
        );
    .Build();
```
The Activity logger is powered by Sql Server and is used to keep track of user activity. The structured logging has a 
short message that is shown to the customer as well as extra data that is used for debugging. 


Here is an example of the SQL table that is generated:
```sql
CREATE TABLE [Logging].[Activity] (
    [Id]              INT            IDENTITY (1, 1) NOT NULL,
    [ActivityType]    NVARCHAR (256) NOT NULL,
    [KeyId]           INT            NOT NULL,
    [KeyType]         NVARCHAR (64)  NOT NULL,
    [AccountId]       INT            NULL,
    [TenantId]        INT            NOT NULL,
    [Message]         NVARCHAR (MAX) NULL,
    [MessageTemplate] NVARCHAR (MAX) NULL,
    [Level]           NVARCHAR (MAX) NULL,
    [TimeStamp]       DATETIME2 (7)  NULL,
    [LogEvent]        NVARCHAR (MAX) NULL,
    [Version]         NVARCHAR (10)  NOT NULL,
    CONSTRAINT [PK_Activity] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_Activity_Key_KeyType]
    ON [Logging].[Activity]([KeyId] ASC, [KeyType] ASC);
```

Here is an example of how the data could appear in Sql in the log event column:
```json
{
    "TimeStamp": "2020-01-22T15:46:27.7267707",
    "Level": "Information",
    "Message": "Logging Activity with Message: \"This is more information\"",
    "MessageTemplate": "Logging Activity with Message: {Structure}",
    "Properties": {
        "Structure": "This is more information",
        "AccountId": 1,
        "AdditionalProperties": {
            "Extra": "Data",
            "MoreExtra": "Data2"
        },
        "LogType": "ActivityLogType",
        "TenantId": 100,
        "ActivityType": "Important",
        "KeyType": "OrderId",
        "KeyId": "260105564",
        "Version": "1.0.0",
        "SourceContext": "ActivityLogger.ExampleActionType"
    }
}
```
### 1. Setting Up The Configuration Files
Add configurations for the connectionString, Tablename and SchemaName
Configuration the Activity Logger with the Type (used to differentiate files in the container) and the version of the 
logs. The version is there in case there is a need for a schema change and different data points.

```json
{
  "ActivityLogger": {
      "ActivityLogType": "ActivityLogType",
      "ActivityLogVersion": "1.0.0",
      "BatchSize": 50,
      "SqlServer": {
        "ConnectionString": "<Fill Out>",
        "TableName": "Activity",
        "SchemaName": "Logging"
      }
   }
}
```

If you are using key store to keep track of the connection string, then you will need to add the connection string 
with a secret name of `TYPE-Infrastructure-ConnectionString`. 

### 2. Use the Logger from the DI
```c#
public class Controller : ControllerBase
{
    private readonly IActivityLogger<ExampleKeyTypeEnum, Controller> _activityLogger;
    
    public Controller(IActivityLogger<ExampleKeyTypeEnum, Controller> activityLogger)
    {
        _activityLogger = activityLogger;
    }
    public void LogData()
    {
        var orderId = new Random((int) DateTime.Now.ToOADate()).Next();
        var accountId = 1;
        _activityLogger.LogActivity(
             ExampleActionType.GetOrder,
             ExampleKeyTypeEnum.OrderId,
             orderId,
             1,
             new { Price = "10.54", ShipDate = "10-21-2019" },
             "Order was placed by {CustomerName} on {OrderDate}",
             "Bill Battson", new DateTime(2019, 10, 15, 0, 0, 0)
        );
    }
}

```


## Update the Logging Level

The Logger Bootstrapper holds the logging level switch. The minimum logging level can be changed in runtime; It defaults to LogEventLevel.Information.
You will need to implement either a UI or endpoint to allow a use to change this value on the fly. The LoggingLevelSwitch object is available via the DI 
so you can inject it into the contructor of any class.

```c#
class LogController
{
	private readonly LoggingLevelSwitch _loggingLevelSwitch;
	
	public LogController(LoggingLevelSwitch loggingLevelSwitch)
	{
		_loggingLevelSwitch = loggingLevelSwitch;
	}
	
	public void SetLogLevel(LogEventLevel logEventLevel)
	{
		_loggingLevelSwitch.MinimumLevel = logEventLevel;	
	}
}
// Change to Debug
(new LogController()).SetLogLevel(LogEventLevel.Debug);

//Change back to Information
(new LogController()).SetLogLevel(LogEventLevel.Information);
```