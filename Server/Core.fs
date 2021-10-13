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
        | ".html" | ".xhtml" | ".htm" | ".xhtm" -> (extension, "text/html")
        | ".md" -> (extension, "text/markdown")
        | ".gmi" | ".gemini" -> (extension, "text/gemini")
        | _ -> (extension, "text/plain")

    let refinePath (pathSegments: string array) =
        let removeTrailingSlash (str: string) =
            str.TrimEnd([|'/'|])

        match Array.length pathSegments with
        | 2 -> Array.last pathSegments |> removeTrailingSlash
        | _ -> 
            let path = Array.skip 0 pathSegments |> String.concat ""
            path.[1..] |> removeTrailingSlash