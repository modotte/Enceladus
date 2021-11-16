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
        
    let retrieveRequestedFile (directoryPath, filename) configuration =
        try
            let fullPath = Path.Combine(configuration.StaticDirectory, directoryPath)
            Ok (Directory.GetFiles(fullPath, $"{filename}.?*") |> Array.tryHead)
        with
        | :? DirectoryNotFoundException as exn ->
            Error exn
            
    let createHeaderResponse response =
        match response.Status with
        | TemporaryFailure
        | PermanentFailure
        | ClientCertificateRequired ->
            response.Stream.Write(Encoding.UTF8.GetBytes($"{getStatusCode response.Status} {response.ErrorMessage.Value}\r\n"))
        | _ -> response.Stream.Write(Encoding.UTF8.GetBytes($"{getStatusCode response.Status} {response.Mime.Value}; \r\n"))

    let createBodyResponse response = response.Stream.Write(File.ReadAllBytes(response.Filename.Value))

    let createOtherPageResponse response = 
        match response.Filename with
        | Some _file ->
            let mime = extractMIMEFromExtension _file
            
            createHeaderResponse { response with Status = Success; Mime = Some mime; ErrorMessage = None }
            createBodyResponse response
            Ok (getStatusCode Success, _file)
            
        | None ->
            let err = "File Not Found"
            createHeaderResponse { response with Status = PermanentFailure; Mime = None; ErrorMessage = Some "File not found" }
            Error err

    let createIndexPageResponse response  = 
        createHeaderResponse { response with Status = Success; Mime = Some "text/gemini"; ErrorMessage = None }
        createBodyResponse response
        Ok (getStatusCode Success, response.Filename.Value)

    let createServerResponse stream configuration parsedClientRequest =
        let response = { Stream = stream; Status = Success; Mime = None; Filename = None; ErrorMessage = None }
        try
            let indexFilePath = Path.Combine(configuration.StaticDirectory, configuration.IndexFile)

            match parsedClientRequest with
            | _ when Uri(parsedClientRequest).LocalPath = "/" && File.Exists(indexFilePath) ->
                createIndexPageResponse { response with Filename = Some indexFilePath }
            | _ ->
                match retrieveRequestedFile (Uri(parsedClientRequest).Segments |> combinePathsFromUri) configuration with
                | Ok _file ->
                    createOtherPageResponse { response with Filename = _file }
                    
                | Error err ->
                    createHeaderResponse { response with Status = PermanentFailure; Mime = None; ErrorMessage = Some "Path not found" }
                    Error err.Message
                
        with
        | :? UnauthorizedAccessException as exn ->
            createHeaderResponse { response with Status = PermanentFailure; Mime = None; ErrorMessage = Some $"Forbidden access. {exn.Message}" }

            Error exn.Message
        | :? UriFormatException as exn ->
            createHeaderResponse { response with Status = PermanentFailure; Mime = None; ErrorMessage = Some $"URI parsing error. {exn.Message}" }
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
       
    let listenClientRequest serverCertificate configuration (client: TcpClient) =
        let sslStream = new SslStream(client.GetStream(), false)

        sslStream.AuthenticateAsServer(serverCertificate, false, true)
        sslStream.ReadTimeout <- configuration.RequestTimeoutDuration
        sslStream.WriteTimeout <- configuration.ResponseTimeoutDuration

        logger.Information($"A client with IP address {retrieveClientIPAddress client} connected..")
        let request = parseClientRequest sslStream
        logger.Information($"A client requested the URI: {request}")

        match createServerResponse sslStream configuration request with
        | Ok (code, page) ->
            logger.Information($"Successful response to {page} with {code} as status code")
        | Error err ->
            logger.Error(err)

        sslStream.Close()
        client.Close()

        logger.Information("Closed last client connection..")

    let runServer configuration =
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
