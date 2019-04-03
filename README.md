# Socreate Extensions Logging

This project is used to add both application insights and cosmos db logging through serilog.

## Add Application Insights Logging
Add the instrument key to the appSettings.json.
```json
{
    "ApplicationInsights" : {
        "InstrumentationKey" : "<Key>"
    }
}
```
Set the UseApplicationInsights Option to true.
```c#
LoggerBootstrapper.InitializeServiceFabricRegistration(
    (serviceName, configuration, addloggingToServiceContext) =>
    {
        // Start Service
        
        // GetContext
        // addloggingToServiceContext(serviceContext);
        
    }, new ServiceFabricLoggerOptions
    {
        ServiceName = "Example",
        ServiceTypeName = "ExampleServiceType",
        UseApplicationInsights = true
    }
);

```


## Add Activity Logging (Powered By Cosmos DB)
The Activity logger is powered by Cosmos DB and is used to keep track of user activity. The structured logging has a 
short message that is shown to the customer as well as extra data that is used for debugging. Here is an example of how
the data will appear in Cosmos:
```json
{
    "EventIdHash": 2837138426,
    "Timestamp": "2019-04-03 22:06:27.121+00:00",
    "Level": "Information",
    "Message": "Did you see that interesting thing? \"This is the extension method\"",
    "MessageTemplate": "Did you see that interesting thing? {InterestingString}",
    "Properties": {
        "InterestingString": "This is the extension method",
        "AdditionalProperties": {
            "Time": "2019-04-03T15:06:27.1212753-07:00"
        },
        "LogType": "ActivityLogType",
        "ActionType": 1,
        "KeySet": {
            "SpecialExampleId": "1134040416"
        },
        "Version": "v1",
        "SourceContext": "ActivityLogger.ExampleActionType"
    },
    "id": "8242363f-2184-c1ae-8dcc-47919eb1ffb4",
    "_rid": "Gdd8AOSaENQDAAAAAAAAAA==",
    "_self": "dbs/Gdd8AA==/colls/Gdd8AOSaENQ=/docs/Gdd8AOSaENQDAAAAAAAAAA==/",
    "_etag": "\"00000000-0000-0000-ea69-7c005d9a01d4\"",
    "_attachments": "attachments/",
    "_ts": 1554329187
}
```

### 1. Implement the Keyset
To implement the activity, the IActivityKeySet will need to implemented. This keyset will be used for searching for the logs.
The Partion Key should be in the KeySet and should be enforced to always be there.

Example:
```
public class ExampleKeySet : IActivityKeySet
{
    public const string SpecialExampleIdKey = "SpecialExampleId";
    public const string AnotherExampleIdKey = "AnotherExampleId";

    public int SpecialExampleId { get; set; }
    public int AnotherExampleId { get; set; }
}
```

### 2. Setting Up The Configuration Files
Add Cosmos DB configurations for the Uri, Key, Database Name and the Collection for the logs.
Configuration the Activity Logger with the Type (used to differenciate files in the container) and the version of the 
logs. The version is there in case there is a need for a schema change and different data points.

```json
{
  "Azure" : {
    "CosmosDb" : {
      "Uri" : "https://localhost:8081",
      "Key" : "<Key>",
      "DatabaseName" : "Examples",
      "CollectionName" : "Logs"
    }
  },
  "ActivityLogger" : {
    "ActivityLogType" : "ActivityLogType",
    "ActivityLogVersion" : "v1"
  }
}
```

### 3. Setup the Services and Options
```c#
-- Program.cs
LoggerBootstrapper.InitializeServiceFabricRegistration(
    (serviceName, configuration, addloggingToServiceContext) =>
    {
        // Start up Service
        
        // GetContext
        // addloggingToServiceContext(serviceContext);
    }, new ServiceFabricLoggerOptions
    {
        ServiceName = "Example",
        ServiceTypeName = "ExampleServiceType",
        UseActivityLogger = true
    }
);

-- Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // If you have a child class of ActivityLogger, then add that type instead
    serviceCollection.AddActivityLogger(typeof(ActivityLogger<>));
}

```

### 4. Use the Logger from the DI
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
        _activityLogger.LogActivity( new ExampleKeySet {SpecialExampleId = randomId}, ExampleActionType.Default,
            new AdditionalData(("Extra", "Data"), ("MoreExtra", "Data2")), "Logging Activity with Message: {Structure}",
            "This is more information");
    }
}

```


## Update the Logging Level

The Logger Bootstrapper holds the logging level switch. The minimum logging level can be changed in runtime; It defaults to LogEventLevel.Information.
You will need to implement either a UI or endpoint to allow a use to change this value on the fly.

```c#
// Change to Debug
LoggerBootstrapper.LoggingLevelSwitch.MinimumLevel = LogEventLevel.Debug;

//Change back to Information
LoggerBootstrapper.LoggingLevelSwitch.MinimumLevel = LogEventLevel.Information;
```