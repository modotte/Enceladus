namespace Enceladus

open System.IO

open System.Net.Security
open MimeMapping

module Core =
    type StatusCode =
        | Input
        | Success
        | Redirect
        | TemporaryFailure
        | PermanentFailure
        | ClientCertificateRequired

    type Response =
        { Stream: SslStream
          Status: StatusCode
          Mime: string option
          Filename: string option
          ErrorMessage: string option }

    let getStatusCode =
        function
        | Input -> 10
        | Success -> 20
        | Redirect -> 30
        | TemporaryFailure -> 40
        | PermanentFailure -> 50
        | ClientCertificateRequired -> 60

    let mimeFromExtension (filename: string) =
        let extension = Path.GetExtension(filename)

        match extension with
        | ".gmi"
        | ".gemini" -> "text/gemini"
        | _ -> MimeUtility.GetMimeMapping(filename)