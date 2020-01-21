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
Set the SendLogDataToApplicationInsights Option to true.
```c#
var host = new HostBuilder()
    .ConfigureLogging(builder => builder.AddServiceLogging(new LoggerOptions {SendLogDataToApplicationInsights = true})
    .Build();

```


## Add Activity Logging (Powered By SqlServer)

Set the SendLogActivityDataToSql Option to true.
```c#
var host = new HostBuilder()
    .ConfigureLogging(builder => builder.AddServiceLogging(new LoggerOptions {SendLogActivityDataToSql = true})
    .Build();
```
The Activity logger is powered by Sql Server and is used to keep track of user activity. The structured logging has a 
short message that is shown to the customer as well as extra data that is used for debugging. 


Here is an example of the SQL table that is generated:
```sql
CREATE TABLE [dbo].[ActivityLog] (
    [Id]              INT            IDENTITY (1, 1) NOT NULL,
    [Key]             INT            NULL,
    [KeyType]         NVARCHAR (64)  NULL,
    [Message]         NVARCHAR (MAX) NULL,
    [MessageTemplate] NVARCHAR (MAX) NULL,
    [Level]           NVARCHAR (MAX) NULL,
    [TimeStamp]       DATETIME2 (7)  NULL,
    [LogEvent]        NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_ActivityLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO
CREATE NONCLUSTERED INDEX [IX_ActivityLog_Key_KeyType]
    ON [dbo].[ActivityLog]([Key] ASC, [KeyType] ASC);
```

Here is an example of how the data could appear in Sql in the log event column:
```json
{
    "TimeStamp": "2020-01-21T11:17:19.7613549",
    "Level": "Information",
    "Message": "Logging Activity with Message: \"This is more information\"",
    "MessageTemplate": "Logging Activity with Message: {Structure}",
    "Properties": {
        "Structure": "This is more information",
        "AdditionalProperties": {
            "Extra": "Data",
            "MoreExtra": "Data2"
        },
        "LogType": "ActivityLogType",
        "KeyType": "UserId",
        "Key": "1285689392",
        "Version": "v1",
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
  "SqlServer": {
     "ConnectionString" : "",
     "TableName" : "ActivityLog",
     "SchemaName" : "dbo"
   },
  "ActivityLogger" : {
    "ActivityLogType" : "ActivityLogType",
    "ActivityLogVersion" : "v1"
  }
}
```


### 2. Use the Logger from the DI
```c#
public class Controller : ControllerBase
{
    private readonly IActivityLogger<CreditController> _activityLogger;
    
    public Controller(IActivityLogger<Controller> activityLogger)
    {
        _activityLogger = activityLogger;
    }
    public void LogData()
    {
        var randomId = new Random((int) DateTime.Now.ToOADate()).Next();
        _activityLogger.LogActivity( randomId, ExampleActionType.Default,
            new AdditionalData(("Extra", "Data"), ("MoreExtra", "Data2")), "Logging Activity with Message: {Structure}",
            "This is more information");
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