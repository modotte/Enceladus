namespace Enceladus

open System.IO

open MimeMapping

module Core =
    type StatusCode =
    | Input
    | Success
    | Redirect
    | TemporaryFailure
    | PermanentFailure
    | ClientCertificateRequired
        
    let getStatusCode = function
    | Input -> 10
    | Success -> 20
    | Redirect -> 30
    | TemporaryFailure -> 40
    | PermanentFailure -> 50
    | ClientCertificateRequired -> 60

    let extractMIMEFromExtension (filename: string) =
        let extension = Path.GetExtension(filename)
        
        match extension with
        | ".gmi" | ".gemini" -> "text/gemini"
        | _ -> MimeUtility.GetMimeMapping(filename)


    let private removeSlashes (uriSegments: string array) =
        uriSegments |> Array.map (fun s -> s.Trim('/'))

    let private asDirectoryPath (uriSegments: string array) =
        Path.Combine(uriSegments .[1 .. uriSegments.Length - 2])

    let private asFilename (uriSegments: string array) = uriSegments .[uriSegments.Length - 1]

    let combinePathsFromUri (uriSegments: string array) =
        let segments = removeSlashes uriSegments
           
        (asDirectoryPath segments, asFilename segments)