namespace EventFlowParser

open System

type Identifier = Identifier of string
type UrlText = UrlText of string
type UrlPart =
    | Text of UrlText
    | Param of Identifier
type Url = Url of UrlPart list
type Endpoint = { Method: string; Url: Url }
//    | GET of Url
//    | POST of Url
//    | PUT of Url
type Send = { Event: Identifier; Topic: Identifier }
type Subscribe = { Topic: Identifier; Function: Identifier }
type FlowElement =
    | Endpoint of Endpoint
    | Send of Send
    | Subscribe of Subscribe
type Flow = Flow of FlowElement list
type Event = { Name: Identifier; Props: (Identifier * string) list }


// implementations
open System.Threading
open System.Xml.Linq
open Constants
open FParsec

module Identifier =
    let create chars =
        let containsInvalid =
            chars
            |> List.exists (fun c -> not (isLetter c || isDigit c || isAnyOf "_" c))
        if containsInvalid then invalidArg "identifier" "Identifier must contain only letters and digits"      
        elif isDigit (chars.[0]) then invalidArg "identifier" "Identifier cannot start with a digit" 
        else Identifier (chars |> Array.ofList |> String)
        
module UrlText =
    let create (str: string) =
        let chars = List.ofSeq str
        let containsInvalid =
            chars
            |> List.exists (fun c -> not (isLetter c || isDigit c || isAnyOf UrlSpecialChars c))
        if containsInvalid then invalidArg "url" "Url must contain only valid characters"      
        else UrlText (chars |> Array.ofList |> String)
module Url =
    let toString (Url parts) =
        "/" +
        (parts
        |> List.map (function
            | Text (UrlText text) | Param (Identifier text) -> text)
        |> String.concat "/")
        
module Endpoint =
    let create (method: string) url =
        match method.ToUpper() with
        | "GET" as m -> { Method = "GET"; Url = url }
        | "POST" -> { Method = "POST"; Url = url }
        | "PUT" -> { Method = "PUT"; Url = url }
        | _ -> invalidArg "HttpMethod" "HttpMethod can be GET, POST or PUT"

module Flow =
    let subscribedTopics (Flow elements) =
        elements
        |> List.choose (function
            | Subscribe sub -> Some sub.Topic
            | Send _ | Endpoint _ -> None)
        |> Set.ofList
        
    let publishedTopics (Flow elements) =
        elements
        |> List.choose (function
            | Send send -> Some send.Topic
            | Subscribe _ | Endpoint _ -> None)
        |> Set.ofList
        
    let allEndpoints (Flow elements) =
        elements
        |> List.choose (function
            | Endpoint endpoint -> Some endpoint
            | Send _ | Subscribe _ -> None)
        |> Set.ofList
        
    let allSubscriptions (Flow elements) =
        elements
        |> List.choose (function
            | Subscribe sub -> Some sub
            | Send _ | Endpoint _ -> None)
        |> Set.ofList
        