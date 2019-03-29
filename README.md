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
    "EventIdHash": 3760679657,
    "Discriminator": "ActivityLog",
    "Timestamp": "2019-03-19 21:32:05.414+00:00",
    "Level": "Information",
    "Message": "Accessed Credit 1",
    "MessageTemplate": "Accessed Credit {Id}",
    "Properties": {
        "Id": 1,
        "LogType": "ActivityLogType",
        "AgentInformation": {
            "AgentId": 123,
            "AgentEmailAddress": "person@aol.com",
            "AgentName": "Billy Jean",
            "IsDeveloper": true,
            "IpAddress": "127.0.0.1"
        },
        "AdditionalProperties": {
            "Amount": "10.50"
        },
        "ActionType": "GetCredit",
        "KeySet": {
            "FinancialAccountId": "1",
            "CreditId": "1"
        },
        "Version": "v1",
        "SourceContext": "Socreate.Api.Service.Controllers.Controller",
        "ActionId": "4a5007bf-6b50-4376-a2a2-d008b26b37b8",
        "ActionName": "Socreate.Api.Service.Controllers.Controller.Get (Socreate.Api.Service)",
        "RequestId": "0HLLCM97HDLNI:00000002",
        "RequestPath": "/api/socreate/credit/1",
        "CorrelationId": null,
        "ConnectionId": "0HLLCM97HDLNI"
    },
    "id": "7dc00b5f-a1b4-5ab8-b4a3-33024901bff6",
    "_rid": "uFBsAOGDq3gCAAAAAAAAAA==",
    "_self": "dbs/uFBsAA==/colls/uFBsAOGDq3g=/docs/uFBsAOGDq3gCAAAAAAAAAA==/",
    "_etag": "\"00000000-0000-0000-dea8-094e4d2c01d4\"",
    "_attachments": "attachments/",
    "_ts": 1553036639
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