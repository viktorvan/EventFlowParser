module EventFlowParser.Printer

open System.Text
let buildArmTemplate filename subs =
    let subscriptionVar (Identifier name) = sprintf "\"%sSubscriptionName\": \"%s\"" name name
    let topicVar (Identifier name) = sprintf "\"%sTopicName\": \"%s\"" name name

    let topicResource { Topic = (Identifier topic); Function = _ } =
        topic |> sprintf """{
            "type": "Microsoft.ServiceBus/namespaces/topics",
            "name": "[concat(parameters('serviceBusName'), '/', variables('%sTopicName'))]",
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
        }"""

    let subscriptionResource { Topic = (Identifier topic); Function = (Identifier func) } = 
        (topic, func, topic) |||> sprintf """{
            "type": "Microsoft.ServiceBus/namespaces/topics/subscriptions",
            "name": "[concat(parameters('serviceBusName'), '/', variables('%sTopicName'), '/', variables('%sSubscriptionName'))]",
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
                "[resourceId('Microsoft.ServiceBus/namespaces/topics', parameters('serviceBusName'), variables('%sTopicName'))]"
            ]
        }""" 
        

    let variableStrings = 
        subs
        |> Set.toList
        |> List.collect (fun (sub: Subscribe) ->
            [ topicVar sub.Topic 
              subscriptionVar sub.Function ])
        |> String.concat ",\n        "

    let resourceStrings =
        subs
        |> Set.toList
        |> List.collect(fun (sub: Subscribe) ->
            [ topicResource sub
              subscriptionResource sub ])    
        |> String.concat ",\n        "          


    let content =
        "{\n    \"variables\": {\n        " +
        variableStrings +
        "\n    },\n    " +
        "\"resources\": [\n        " +
        resourceStrings +
        "\n    ]\n}"
    System.IO.File.WriteAllText(filename, content)
    
let buildFunctions filename (endpoints: Set<Endpoint>) (subs: Set<Subscribe>) =
    let toTitleCase = System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase
    let endpointName { Method = method; Url = (Url parts) } =
        let urlPartsString =
            parts
            |> List.map (function
                | Text (UrlText text) | Param (Identifier text) -> text)
            |> List.map toTitleCase 
            |> String.concat ""
        method + urlPartsString
        
    let httpFunction endpoint =
        let name = endpointName endpoint
        sprintf """
    [FunctionName("%s")]
    public async Task %s([HttpTrigger(AuthorizationLevel.Function, "%s", Route = "%s")] HttpRequest req)
    {
        throw new NotImplementedException();
    }""" name name endpoint.Method (Url.toString endpoint.Url)
        
    let serviceBusFunction { Topic = (Identifier topic); Function = (Identifier func) } =
        sprintf """
    [FunctionName("%s")]
    public async Task %s([ServiceBusTrigger("%s", "%s", Connection = KnownConnections.Subscriber)] Message[] messages)
    {
        throw new NotImplementedException();
    }""" func func topic func
    
        
    let httpFunctionsStrings =
        endpoints
        |> Set.toList
        |> List.map httpFunction
        |> String.concat "\n"
        
    let serviceBusFunctionsStrings =
        subs
        |> Set.toList
        |> List.map serviceBusFunction
        |> String.concat "\n"
        
    let content =
        (httpFunctionsStrings, serviceBusFunctionsStrings) ||> sprintf """using System;
using System.Threading.Tasks;
using ComAround.Backend.Data.AzureServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

public class Functions
{
%s
%s
}
""" 
    System.IO.File.WriteAllText(filename, content)
    
