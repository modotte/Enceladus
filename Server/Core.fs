namespace Enceladus

open System.IO

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
        | ".html" | ".xhtml" | ".htm" | ".xhtm" -> "text/html"
        | ".png" -> "image/png"
        | ".jpeg" | ".jpg" -> "image/jpeg"
        | ".md" -> "text/markdown"
        | ".gmi" | ".gemini" -> "text/gemini"
        | _ -> "text/plain"

    let refinePath (pathSegments: string array) =
        let removeTrailingSlashes (str: string) = str.Trim('/')
        let segments =
            pathSegments
            |> Array.skip 1
            |> Array.map removeTrailingSlashes
        
        let directoryPath =
            segments
            |> Array.rev
            |> Array.skip 1
            |> Array.rev
            
        (Path.Combine(directoryPath), Array.last segments)
            