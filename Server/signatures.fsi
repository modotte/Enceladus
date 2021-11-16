



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
        
        val extractMIMEFromExtension: filename: string -> string
        
        val private removeSlashes: uriSegments: string array -> string[]
        
        val private asDirectoryPath: uriSegments: string array -> string
        
        val private asFilename: uriSegments: string array -> string
        
        val combinePathsFromUri: uriSegments: string array -> string * string

namespace Enceladus
    
    module Server =
        
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
        
        val retrieveRequestedFile:
          directoryPath: string * filename: 'a
          -> configuration: ServerConfiguration
            -> Result<string option,System.IO.DirectoryNotFoundException>
        
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
        
        val retrieveClientIPAddress:
          client: System.Net.Sockets.TcpClient -> System.Net.IPAddress
        
        val listenClientRequest:
          serverCertificate: System.Security.Cryptography.X509Certificates.X509Certificate
          -> configuration: ServerConfiguration
          -> client: System.Net.Sockets.TcpClient -> unit
        
        val runServer: configuration: ServerConfiguration -> unit

namespace Enceladus
    
    module Program =
        
        val main: string[] -> int

