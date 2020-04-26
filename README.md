# EventFlowParser

A project that parses Event Flow DSL to corresponding code files.

## DSL Notation

```
API endpoint:                       HTTP-METHOD /theEndpointName/{parameter}
Topic:                              @theTopicName
Message:                            [TheMessageName]
Functions:                          f:{TheFunctionName}
Send service bus message:           => [Message] @topic
Send message with fan-out:          => *[Message] @topic
Functions Consuming messages:       -> @topic f:{TheFunctionName}
```

## Generated code files
#### Functions.cs

A C# class with functions corresponding to the http endpoints and service bus triggers defined in the flow.

#### generated.json
Part of an arm-template with variables and resource for topics and subscrptions:

## Usage

### Input
The project can parse input as a file, or as a string parameter

1. input file:

```
// input.efd
POST /content                                     => *translations [CreateTranslationValidated] @createTranslationValidated
-> @createTranslationValidated f:{UpsertNode}     => [TranslationNodeCreated] @translationNodeCreated
-> @translationNodeCreated f:{CreateTranslation}  => [TranslationChanged] @translationChanged
-> @translationChanged f:{UpdateSearch}

> dotnet run --project src/EventFlowParser -- input.efd
```

2. string parameter

```
> dotnet run --project src/EventFlowParser -- "POST /content => *translations [CreateTranslationValidated] @createTranslationValidated
-> @createTranslationValidated f:{UpsertNode} => [TranslationNodeCreated] @translationNodeCreated
-> @translationNodeCreated f:{CreateTranslation} => [TranslationChanged] @translationChanged
-> @translationChanged f:{UpdateSearch}"

```

### Output

```csharp
using System;
using System.Threading.Tasks;
using ComAround.Backend.Data.AzureServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

public class Functions
{

    [FunctionName("POSTContent")]
    public async Task POSTContent([HttpTrigger(AuthorizationLevel.Function, "POST", Route = "/content")] HttpRequest req)
    {
        throw new NotImplementedException();
    }

    [FunctionName("UpsertNode")]
    public async Task UpsertNode([ServiceBusTrigger("createTranslationValidated", "UpsertNode", Connection = KnownConnections.Subscriber)] Message[] messages)
    {
        throw new NotImplementedException();
    }

    [FunctionName("UpdateSearch")]
    public async Task UpdateSearch([ServiceBusTrigger("translationChanged", "UpdateSearch", Connection = KnownConnections.Subscriber)] Message[] messages)
    {
        throw new NotImplementedException();
    }

    [FunctionName("CreateTranslation")]
    public async Task CreateTranslation([ServiceBusTrigger("translationNodeCreated", "CreateTranslation", Connection = KnownConnections.Subscriber)] Message[] messages)
    {
        throw new NotImplementedException();
    }
}
```

```json
{
    "variables": {
        "createTranslationValidatedTopicName": "createTranslationValidated",
        "UpsertNodeSubscriptionName": "UpsertNode",
        "translationChangedTopicName": "translationChanged",
        "UpdateSearchSubscriptionName": "UpdateSearch",
        "translationNodeCreatedTopicName": "translationNodeCreated",
        "CreateTranslationSubscriptionName": "CreateTranslation"
    },
    "resources": [
        {
            "type": "Microsoft.ServiceBus/namespaces/topics",
            "name": "[concat(parameters('serviceBusName'), '/', variables('createTranslationValidatedTopicName'))]",
            "apiVersion": "2017-04-01",
            "location": "[resourceGroup().location]",
            "scale": null,
            "properties": {
                "defaultMessageTimeToLive": "P14D",
                "maxSizeInMegabytes": 1024,
                "requiresDuplicateDetection": true,
                "duplicateDetectionHistoryTimeWindow": "PT10M",
                "enableBatchedOperations": true,
                "supportOrdering": false,
                "autoDeleteOnIdle": "P10675199DT2H48M5.4775807S",
                "enablePartitioning": true,
                "enableExpress": false
            },
            "dependsOn": [
                "[resourceId('Microsoft.ServiceBus/namespaces', parameters('serviceBusName'))]"
            ]
        },
        {
            "type": "Microsoft.ServiceBus/namespaces/topics/subscriptions",
            "name": "[concat(parameters('serviceBusName'), '/', variables('createTranslationValidatedTopicName'), '/', variables('UpsertNodeSubscriptionName'))]",
            "apiVersion": "2017-04-01",
            "location": "[resourceGroup().location]",
            "properties": {
                "lockDuration": "PT1M",
                "requiresSession": false,
                "defaultMessageTimeToLive": "P10675199DT2H48M5.4775807S",
                "deadLetteringOnMessageExpiration": true,
                "messageCount": 0,
                "maxDeliveryCount": 10,
                "status": "Active",
                "enableBatchedOperations": true,
                "requiresDuplicateDetection": true,
                "duplicateDetectionHistoryTimeWindow": "P1D",
                "autoDeleteOnIdle": "P10675199DT2H48M5.4775807S"
            },
            "dependsOn": [
                "[resourceId('Microsoft.ServiceBus/namespaces', parameters('serviceBusName'))]",
                "[resourceId('Microsoft.ServiceBus/namespaces/topics', parameters('serviceBusName'), variables('createTranslationValidatedTopicName'))]"
            ]
        },
        {
            "type": "Microsoft.ServiceBus/namespaces/topics",
            "name": "[concat(parameters('serviceBusName'), '/', variables('translationChangedTopicName'))]",
            "apiVersion": "2017-04-01",
            "location": "[resourceGroup().location]",
            "scale": null,
            "properties": {
                "defaultMessageTimeToLive": "P14D",
                "maxSizeInMegabytes": 1024,
                "requiresDuplicateDetection": true,
                "duplicateDetectionHistoryTimeWindow": "PT10M",
                "enableBatchedOperations": true,
                "supportOrdering": false,
                "autoDeleteOnIdle": "P10675199DT2H48M5.4775807S",
                "enablePartitioning": true,
                "enableExpress": false
            },
            "dependsOn": [
                "[resourceId('Microsoft.ServiceBus/namespaces', parameters('serviceBusName'))]"
            ]
        },
        {
            "type": "Microsoft.ServiceBus/namespaces/topics/subscriptions",
            "name": "[concat(parameters('serviceBusName'), '/', variables('translationChangedTopicName'), '/', variables('UpdateSearchSubscriptionName'))]",
            "apiVersion": "2017-04-01",
            "location": "[resourceGroup().location]",
            "properties": {
                "lockDuration": "PT1M",
                "requiresSession": false,
                "defaultMessageTimeToLive": "P10675199DT2H48M5.4775807S",
                "deadLetteringOnMessageExpiration": true,
                "messageCount": 0,
                "maxDeliveryCount": 10,
                "status": "Active",
                "enableBatchedOperations": true,
                "requiresDuplicateDetection": true,
                "duplicateDetectionHistoryTimeWindow": "P1D",
                "autoDeleteOnIdle": "P10675199DT2H48M5.4775807S"
            },
            "dependsOn": [
                "[resourceId('Microsoft.ServiceBus/namespaces', parameters('serviceBusName'))]",
                "[resourceId('Microsoft.ServiceBus/namespaces/topics', parameters('serviceBusName'), variables('translationChangedTopicName'))]"
            ]
        },
        {
            "type": "Microsoft.ServiceBus/namespaces/topics",
            "name": "[concat(parameters('serviceBusName'), '/', variables('translationNodeCreatedTopicName'))]",
            "apiVersion": "2017-04-01",
            "location": "[resourceGroup().location]",
            "scale": null,
            "properties": {
                "defaultMessageTimeToLive": "P14D",
                "maxSizeInMegabytes": 1024,
                "requiresDuplicateDetection": true,
                "duplicateDetectionHistoryTimeWindow": "PT10M",
                "enableBatchedOperations": true,
                "supportOrdering": false,
                "autoDeleteOnIdle": "P10675199DT2H48M5.4775807S",
                "enablePartitioning": true,
                "enableExpress": false
            },
            "dependsOn": [
                "[resourceId('Microsoft.ServiceBus/namespaces', parameters('serviceBusName'))]"
            ]
        },
        {
            "type": "Microsoft.ServiceBus/namespaces/topics/subscriptions",
            "name": "[concat(parameters('serviceBusName'), '/', variables('translationNodeCreatedTopicName'), '/', variables('CreateTranslationSubscriptionName'))]",
            "apiVersion": "2017-04-01",
            "location": "[resourceGroup().location]",
            "properties": {
                "lockDuration": "PT1M",
                "requiresSession": false,
                "defaultMessageTimeToLive": "P10675199DT2H48M5.4775807S",
                "deadLetteringOnMessageExpiration": true,
                "messageCount": 0,
                "maxDeliveryCount": 10,
                "status": "Active",
                "enableBatchedOperations": true,
                "requiresDuplicateDetection": true,
                "duplicateDetectionHistoryTimeWindow": "P1D",
                "autoDeleteOnIdle": "P10675199DT2H48M5.4775807S"
            },
            "dependsOn": [
                "[resourceId('Microsoft.ServiceBus/namespaces', parameters('serviceBusName'))]",
                "[resourceId('Microsoft.ServiceBus/namespaces/topics', parameters('serviceBusName'), variables('translationNodeCreatedTopicName'))]"
            ]
        }
    ]
}
```






