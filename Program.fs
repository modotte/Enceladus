open System
open System.Net.Security
open System.Security.Authentication
open System.Security.Cryptography.X509Certificates
open System.Net
open System.Net.Sockets
open System.IO
open System.Text
open Serilog

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
    
let writeHeaderResponse (sslStream: SslStream) (statusCode: StatusCode) =
    match statusCode with
    | TemporaryFailure | PermanentFailure | ClientCertificateRequired ->
        sslStream.Write(Encoding.UTF8.GetBytes($"{getStatusCode statusCode} An error occured\r\n"))
    
    // TODO: Handle multiple MIME types.
    | _ -> sslStream.Write(Encoding.UTF8.GetBytes($"{getStatusCode statusCode} text/gemini; \r\n"))
    
let writeBodyResponse (sslStream: SslStream) (text: string) =
    sslStream.Write(Encoding.UTF8.GetBytes($"{text}"))

type Server() =
    let port = 1965
    let staticDirectory = "public"
    let MAX_BUFFER_LENGTH = 1048
    let logger = LoggerConfiguration().WriteTo.Console().CreateLogger()
     
    member this.ReadClientRequest(stream: SslStream) =
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

        messageData.ToString()
        
    member this.ReturnFile(path: string) =
        raise(NotImplementedException())
                
    member this.HandleClient(client: TcpClient, serverCertificate: X509Certificate2) =
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
                | message ->
                    match message with
                    | m when Uri(m).LocalPath = "/" && File.Exists($"{staticDirectory}/index.gmi") -> 
                        writeHeaderResponse sslStream Success
                        logger.Information("Sent header")
                        try
                            writeBodyResponse sslStream (File.ReadAllText($"{staticDirectory}/index.gmi"))
                            logger.Information("Send body")
                        with
                        | :? IOException as ex ->
                            logger.Error($"There was an error during file loading: {ex.Message}")

                    | m when File.Exists($"{staticDirectory}/{Uri(m).LocalPath}.gmi") ->
                        logger.Information($"{staticDirectory}/{Uri(m).LocalPath}.gmi")
                        
                        writeHeaderResponse sslStream Success
                        logger.Information("Sent header")
                        
                        writeBodyResponse sslStream (File.ReadAllText($"{staticDirectory}/{Uri(m).LocalPath}.gmi"))
                        logger.Information("Send body")
                    | _ ->
                        writeHeaderResponse sslStream PermanentFailure
                        logger.Error("Path doesn't exist!")
                
            with
            | :? AuthenticationException as ex ->
                logger.Error($"Exception: {ex.Message}")
                if ex.InnerException <> null then
                    logger.Information($"Inner exception: {ex.InnerException.Message}")
                
                sslStream.Close()
                client.Close()
        finally
        sslStream.Close()
        client.Close()
        
        logger.Information("Closed last client connection..")
            
    member this.RunServer(serverCertificate: X509Certificate2) =
        let listener = TcpListener(IPAddress.Any, port)
        listener.Start()
        while true do
            logger.Information($"Waiting for a client to connect at port {port}")
            let client = listener.AcceptTcpClient()
            this.HandleClient(client, serverCertificate)
            
    member this.DisplayUsage() =
        printfn "enceladus <CERT_FILE.pfx> <PASSWORD>"
        printfn "or from dotnet: dotnet run -- <CERT_FILE.pfx> <PASSWORD>"
            
       
[<EntryPoint>]
let main argv =
    let server = Server()
    
    if argv.Length < 2 then
        server.DisplayUsage()
        Environment.Exit(-1)

    let certificateFile = argv.[0]
    let certificatePassword = argv.[1]
    
    server.RunServer(new X509Certificate2(certificateFile, certificatePassword))
    
    0