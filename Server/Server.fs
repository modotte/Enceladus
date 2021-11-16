namespace Enceladus

open System
open System.Net.Security
open System.Security.Cryptography.X509Certificates
open System.Security.Cryptography
open System.Net
open System.Net.Sockets
open System.IO
open System.Text

open Serilog
open Enceladus.Core

module Server =
    type ServerConfiguration = {
        CertificatePFXFile: string
        CertificatePassword: string
        RequestTimeoutDuration: int
        ResponseTimeoutDuration: int
        Host: string
        Port: int
        IndexFile: string
        StaticDirectory: string
    }
        
    let retrieveRequestedFile (directoryPath: string, filename: string) (configuration: ServerConfiguration) =
        try
            let fullPath = Path.Combine(configuration.StaticDirectory, directoryPath)
            Ok (Directory.GetFiles(fullPath, $"{filename}.?*") |> Array.tryHead)
        with
        | :? DirectoryNotFoundException as exn ->
            Error exn

    let createHeaderResponse (sslStream: SslStream) (statusCode: StatusCode) (mime: string option) (errorMessage: string option) =
        match statusCode with
        | TemporaryFailure
        | PermanentFailure
        | ClientCertificateRequired ->
            sslStream.Write(Encoding.UTF8.GetBytes($"{getStatusCode statusCode} {errorMessage.Value}\r\n"))
        | _ -> sslStream.Write(Encoding.UTF8.GetBytes($"{getStatusCode statusCode} {mime.Value}; \r\n"))

    let createBodyResponse (sslStream: SslStream) (filename: string) = sslStream.Write(File.ReadAllBytes(filename))

    let createOtherPageResponse (sslStream: SslStream) (filename: string option) = 
        match filename with
        | Some _file ->
            let mime = extractMIMEFromExtension _file
            
            createHeaderResponse sslStream StatusCode.Success (Some mime) None
            createBodyResponse sslStream _file
            
            Ok (getStatusCode StatusCode.Success, _file)
        | None ->
            let err = "File Not Found"
            createHeaderResponse sslStream PermanentFailure None (Some err)

            Error err

    let createIndexPageResponse  (sslStream: SslStream) (indexFilePath: string) =
        createHeaderResponse sslStream StatusCode.Success (Some "text/gemini") None
        createBodyResponse sslStream indexFilePath
                    
        Ok (getStatusCode StatusCode.Success, indexFilePath)

    let createServerResponse (sslStream: SslStream) (configuration: ServerConfiguration) (message: string) =
        try
            let indexFilePath = Path.Combine(configuration.StaticDirectory, configuration.IndexFile)

            match message with
            | _ when Uri(message).LocalPath = "/" && File.Exists(indexFilePath) ->
                createIndexPageResponse sslStream indexFilePath
            | _ ->
                match retrieveRequestedFile (Uri(message).Segments |> combinePathsFromUri) configuration with
                | Ok file ->
                    createOtherPageResponse sslStream file
                | Error err ->
                    createHeaderResponse sslStream PermanentFailure None (Some "Path Not Found")

                    Error err.Message
                
        with
        | :? UnauthorizedAccessException as exn ->
            createHeaderResponse sslStream PermanentFailure None (Some $"Forbidden access. {exn.Message}")

            Error exn.Message
        | :? UriFormatException as exn ->
            createHeaderResponse sslStream PermanentFailure None (Some $"URI error. {exn.Message}")

            Error exn.Message


    let logger = LoggerConfiguration().WriteTo.Console().CreateLogger()

    let parseClientRequest (sslStream: SslStream) =
        let maxBufferLength = 4096
        let mutable buffer = Array.zeroCreate maxBufferLength
        let message = StringBuilder()
        let mutable bytes = -1

        while bytes <> 0 do
            bytes <- sslStream.Read(buffer, 0, buffer.Length)
            let decoder = Encoding.UTF8.GetDecoder()
            let mutable characters = Array.zeroCreate (decoder.GetCharCount(buffer, 0, bytes))
            decoder.GetChars(buffer, 0, bytes, characters, 0) |> ignore
            message.Append(characters) |> ignore

            match message.ToString().IndexOf("\r\n") with
            | bytesCount when bytesCount <> -1 -> bytes <- 0
            | _ -> ()

        string message

    let retrieveClientIPAddress (client: TcpClient) =
        let endpoint = client.Client.RemoteEndPoint :?> IPEndPoint
        
        endpoint.Address
        
    let listenClientRequest (serverCertificate: X509Certificate2) (configuration: ServerConfiguration) (client: TcpClient) =
        let sslStream = new SslStream(client.GetStream(), false)

        sslStream.AuthenticateAsServer(serverCertificate, false, true)
        sslStream.ReadTimeout <- configuration.RequestTimeoutDuration
        sslStream.WriteTimeout <- configuration.ResponseTimeoutDuration

        logger.Information($"A client with IP address {retrieveClientIPAddress client} connected..")
        let message = parseClientRequest sslStream
        logger.Information($"A client requested the URI: {message}")

        match createServerResponse sslStream configuration message with
        | Ok (code, page) ->
            logger.Information($"Successful response to {page} with {code} as status code")
        | Error err ->
            logger.Error(err)

        sslStream.Close()
        client.Close()

        logger.Information("Closed last client connection..")

    let runServer (configuration: ServerConfiguration) =
        try
            let serverCertificate = new X509Certificate2(configuration.CertificatePFXFile, configuration.CertificatePassword)
            let host = configuration.Host
            let port = configuration.Port
            let hostInfo = Dns.GetHostEntry(host)
            let ipAddress = hostInfo.AddressList.[0]
            let listener = TcpListener(ipAddress, port)
            listener.Start()

            let rec run state =
                logger.Information($"Waiting for a client to connect at gemini://{host}:{port}/")
                let client = listener.AcceptTcpClient()
                listenClientRequest serverCertificate configuration client

                (run state)

            (run true)
        with
        | :? CryptographicException as exn ->
            logger.Error($"SSL certificate validation error! Error: {exn.Message}")
            Environment.Exit(1)
