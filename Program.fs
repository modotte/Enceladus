open System
open System.Net.Security
open System.Security.Authentication
open System.Security.Cryptography.X509Certificates
open System.Net
open System.Net.Sockets
open System.Text
open Serilog

type StatusCode =
    | Input | Success | Redirect
    | TemporaryFailure | PermanentFailure | ClientCertificateRequired

let getStatusCode = function
    | Input -> 10
    | Success -> 20
    | Redirect -> 30
    | TemporaryFailure -> 40
    | PermanentFailure -> 50
    | ClientCertificateRequired -> 60

type Server() =
    let port = 1965
    let mutable serverCertificate = null
    let logger = LoggerConfiguration().WriteTo.Console().CreateLogger()
     
    member this.ReadClientRequest(stream: SslStream) =
        let MAX_BUFFER_LENGTH = 1048
        let mutable buffer = Array.zeroCreate MAX_BUFFER_LENGTH
        let messageData = StringBuilder()
        let mutable bytes = -1
        
        while bytes <> 0 do
            bytes <- stream.Read(buffer, 0, buffer.Length)
            let decoder = Encoding.UTF8.GetDecoder()
            let mutable chars = Array.zeroCreate(decoder.GetCharCount(buffer, 0, bytes))
            decoder.GetChars(buffer, 0, bytes, chars, 0) |> ignore
            messageData.Append(chars) |> ignore
            if messageData.ToString().IndexOf("\r\n") <> -1 then
                bytes <- 0
            else
                ()
            
        match Uri(messageData.ToString()).IsWellFormedOriginalString() with
        | true -> Some(messageData.ToString())
        | false -> None
                
    member this.HandleClient(client: TcpClient) =
        let sslStream = new SslStream(client.GetStream(), false)
        try
            try
                let timeoutDuration = 5000
                sslStream.AuthenticateAsServer(serverCertificate, false, true)
                sslStream.ReadTimeout <- timeoutDuration
                sslStream.WriteTimeout <- timeoutDuration
                
                logger.Information("A client connected..")
                let messageData = this.ReadClientRequest(sslStream)
                logger.Information("A client requested some resources..")
                
                match messageData with
                | Some message ->
                    sslStream.Write(Encoding.UTF8.GetBytes($"{getStatusCode Success} text/gemini; charset=utf8 \r\n"))
                    
                    sslStream.Write(Encoding.UTF8.GetBytes($"Received request: {message}\r\n"))
                    sslStream.Write(Encoding.UTF8.GetBytes("Hello! 你好 \r\n"))
                    sslStream.Write(Encoding.UTF8.GetBytes("=> https://google.com Google Search Engine \r\n"))
                    
                | _ ->
                    logger.Error("Found an error when parsing a client request")
                    sslStream.Write(Encoding.UTF8.GetBytes($"{getStatusCode PermanentFailure} text/gemini; charset=utf8 \r\n"))
                    sslStream.Write(Encoding.UTF8.GetBytes($"Error: {getStatusCode PermanentFailure}. There was a problem occured. Please try again. \r\n"))
                
            with
            | :? AuthenticationException as ex ->
                printfn $"Exception: {ex.Message}"
                if ex.InnerException <> null then
                    logger.Information($"Inner exception: {ex.InnerException.Message}")
                
                sslStream.Close()
                client.Close()
        finally
        sslStream.Close()
        client.Close()
        
        logger.Information("Closed last client connection..")
            
    member this.RunServer(certificate: string, certificatePassword: string) =
        serverCertificate <- new X509Certificate2(certificate, certificatePassword)
        let listener = TcpListener(IPAddress.Any, port)
        listener.Start()
        while true do
            logger.Information($"Waiting for a client to connect at port {port}")
            let client = listener.AcceptTcpClient()
            this.HandleClient(client)
            
    member this.DisplayUsage() =
        printfn "enceladus <CERT_FILE.pfx> <PASSWORD>"
        printfn "or from dotnet: dotnet run -- <CERT_FILE.pfx> <PASSWORD>"
            
       
[<EntryPoint>]
let main argv =
    let server = Server()
    
    if argv.Length < 2 then
        server.DisplayUsage()
        Environment.Exit(-1)

    let certificate = argv.[0]
    let certificatePassword = argv.[1]
    server.RunServer(certificate, certificatePassword)
    
    0