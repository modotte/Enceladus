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

    let getMIMETypeFromExtension (filename: string) =
        let extension = Path.GetExtension(filename)
        match extension with
        | ".gmi" | ".gemini" -> "text/gemini"
        | _ -> MimeUtility.GetMimeMapping(filename)

    let getPathsFromUri (uriSegments: string array) =
        let withoutSlashes = uriSegments |> Array.map (fun s -> s.Trim('/'))        
        let directoryPath = Path.Combine(withoutSlashes .[1 .. withoutSlashes.Length - 2])
        let filename = withoutSlashes .[withoutSlashes.Length - 1]
           
        directoryPath, filename