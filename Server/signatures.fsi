



namespace Enceladus
    
    module Core =
        
        type StatusCode =
            | Input
            | Success
            | Redirect
            | TemporaryFailure
            | PermanentFailure
            | ClientCertificateRequired
        
        type Response =
            {
              Stream: System.Net.Security.SslStream
              Status: StatusCode
              Mime: string option
              Filename: string option
              ErrorMessage: string option
            }
        
        val getStatusCode: _arg1: StatusCode -> int
        
        val mimeFromExtension: filename: string -> string

namespace Enceladus
    
    module Server =
        
        val inline (<!>) :
          x:  ^a -> f: ('c -> 'd) ->  ^b
            when (FSharpPlus.Control.Map or  ^a or  ^b) :
                   (static member Map:
                      ( ^a * ('c -> 'd)) * FSharpPlus.Control.Map ->  ^b)
        
        val inline (|>!) : x: 'a -> f: ('a -> unit) -> 'a
        
        type ServerConfiguration =
            {
              CertificatePFXFile: string
              CertificatePassword: string
              RequestTimeoutDuration: int
              ResponseTimeoutDuration: int
              Host: string
              Port: int
              IndexFile: string
              StaticDirectory: string
            }
        
        val createHeaderResponse: response: Core.Response -> unit
        
        val createBodyResponse: response: Core.Response -> unit
        
        val createOtherPageResponse:
          response: Core.Response -> Result<(int * string),string>
        
        val createIndexPageResponse:
          response: Core.Response -> Result<(int * string),'a>
        
        val createServerResponse:
          stream: System.Net.Security.SslStream
          -> configuration: ServerConfiguration -> parsedClientRequest: string
            -> Result<(int * string),string>
        
        val logger: Serilog.Core.Logger
        
        val parseClientRequest:
          sslStream: System.Net.Security.SslStream -> string
        
        val getClientIPAddress:
          client: System.Net.Sockets.TcpClient -> System.Net.IPAddress
        
        val listenClientRequest:
          serverCertificate: System.Security.Cryptography.X509Certificates.X509Certificate
          -> configuration: ServerConfiguration
          -> client: System.Net.Sockets.TcpClient -> unit
        
        val runServer: configuration: ServerConfiguration -> unit

namespace Enceladus
    
    module Program =
        
        val main: string[] -> int

