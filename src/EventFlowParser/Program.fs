// Learn more about F# at http://fsharp.org

open System
open System.IO
open EventFlowParser


[<EntryPoint>]
let main argv =
    if argv.Length < 1 then printfn "input argument missing" 
    let source = argv.[0]
    
    printfn "starting"
    
    let input = if File.Exists source then File.ReadAllText source else source
    
    let flow = 
        match Parser.parseEventFlow input with
        | Ok flow -> flow
        | Error msg -> sprintf "Failed to parse EventFlow: %s" msg |> failwith
     
    // TODO validate published topics = subscribed topics
    let unSubscribed = (Flow.publishedTopics flow) - Flow.subscribedTopics flow
    if unSubscribed.Count > 0 then
        printfn "There are %i unsubscribed topics: %A" (unSubscribed |> Set.count) unSubscribed
    
    let allSubs = Flow.allSubscriptions flow
    let allEndpoints = Flow.allEndpoints flow
    
    let outDir = "output"
    Directory.CreateDirectory outDir |> ignore
    let armFilename = sprintf "%s/generated.json" outDir
    Printer.buildArmTemplate armFilename (Flow.allSubscriptions flow)
    printfn "Wrote arm-template to %s" armFilename
    
    let functionsFilename = sprintf "%s/Functions.cs" outDir
    Printer.buildFunctions functionsFilename allEndpoints allSubs
    printfn "Wrote functions to %s" functionsFilename

    0 // return an integer exit code
