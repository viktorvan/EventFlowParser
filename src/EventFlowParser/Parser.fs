module EventFlowParser.Parser

open FParsec
open Constants

module Internal =
    type UserState = unit
    type Parser<'T> = Parser<'T,UserState>


    let str = pstringCI
    let ws = spaces

    let pLetter = satisfy isLetter
    let pDigit = satisfy isDigit
    let pUnderscore : Parser<_> = str "_" |>> char
    let pIdentifier : Parser<_> = 
        (pLetter <|> pUnderscore) .>>. (many (choice [pLetter; pDigit; pUnderscore])) |>> fun (fst, rest) -> fst::rest |> Identifier.create
    let pEventName : Parser<_> =
        str "[" >>. ws >>. pIdentifier .>> ws .>> str "]"

    let pFunctionName : Parser<_> =
        let pFunctionSuffix = str "f" >>. ws >>. str ":" .>> ws .>> str "{"
        pFunctionSuffix >>. ws >>. pIdentifier .>> ws .>> str "}"

    let pTopic : Parser<_> =
        str "@" >>. ws >>. pIdentifier .>> ws

    let pEndOfUrlPart : Parser<_> = opt (followedBy (pchar '/'))
    let pUrlText : Parser<_> =
        let pUrlSpecial = satisfy (isAnyOf UrlSpecialChars)
        (many1Chars (choice [pLetter; pDigit; pUrlSpecial])) .>> pEndOfUrlPart |>> UrlText.create

    let pUrlParam : Parser<Identifier> =
        str "{" >>. pIdentifier .>> str "}" .>> pEndOfUrlPart

    let pUrlPart : Parser<UrlPart> =
        choice [ pUrlParam |>> Param ; pUrlText |>> EventFlowParser.Text]

    let pUrlSlash : Parser<_> =
        pchar '/' .>> followedBy (choice [ pUrlText |>> ignore ; pUrlParam |>> ignore ])
    let pUrl : Parser<Url> = 
        pUrlSlash >>. sepEndBy pUrlPart (pchar '/') |>> Url

    let pEndpoint : Parser<_> = 
        let pMethod : Parser<_> = choice [ str "GET"; str "POST"; str "PUT" ]
        pMethod .>> ws .>>. pUrl |>> fun (method, url) -> Endpoint.create method url

    let pSender : Parser<_> =
        let pSendArrow = str "=>"
        let pSenderVariable = (skipString "*" >>. ws >>. pIdentifier |>> ignore) <|> (skipString "*") .>> ws
        let pSender' =
            pEventName .>> ws .>>. pTopic |>> (fun (e, t) -> { Event = e; Topic = t})

        pSendArrow >>. ws >>. (many pSenderVariable) >>. ws >>. pSender'    

    let pSubscriber : Parser<_> =
        str "->" >>. ws >>. pTopic .>> ws .>>. pFunctionName |>> (fun (t, f) -> { Topic = t; Function = f})

    let pFlowElement : Parser<_> =
        choice [ (pSender) |>> Send ; (pSubscriber) |>> Subscribe ; pEndpoint |>> Endpoint ]

    let pFlow : Parser<_> =
        sepEndBy pFlowElement ws .>> eof |>> Flow
        
let parseEventFlow str = 
    match run Internal.pFlow str with
    | Success (result,_,_) -> Result.Ok result
    | Failure (msg,_,_) -> Result.Error msg