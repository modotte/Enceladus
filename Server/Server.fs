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
    type ClientHandlingResult =
        | Success of int * string
        | FileDoesntExistError of string
        | PathDoesntExistError of DirectoryNotFoundException
        | UnauthorizedAccessError of UnauthorizedAccessException
        | UriFormatError of UriFormatException

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
        
    let getFile (directoryPath: string, filename: string) (configuration: ServerConfiguration) =
        try
            let path = Path.Combine(configuration.StaticDirectory, directoryPath)
            Ok (Directory.GetFiles(path, $"{filename}.?*") |> Array.tryHead)
        with
        | :? DirectoryNotFoundException as exn ->
            Error exn

    let writeHeaderResponse (sslStream: SslStream) (statusCode: StatusCode) (mime: string option) (errorMessage: string option) =
        match statusCode with
        | TemporaryFailure
        | PermanentFailure
        | ClientCertificateRequired ->
            sslStream.Write(Encoding.UTF8.GetBytes($"{getStatusCode statusCode} {errorMessage.Value}\r\n"))
        | _ -> sslStream.Write(Encoding.UTF8.GetBytes($"{getStatusCode statusCode} {mime.Value}; \r\n"))

    let writeBodyResponse (sslStream: SslStream) (filename: string) = sslStream.Write(File.ReadAllBytes(filename))

    let nonIndexPageResponse (sslStream: SslStream) (file: string option) = 
        match file with
        | Some _file ->
            let mime = getMIMETypeFromExtension _file
            
            writeHeaderResponse sslStream StatusCode.Success (Some mime) None
            writeBodyResponse sslStream _file
            
            Success (getStatusCode StatusCode.Success, _file)
        | None ->
            let errorMessage = "File Not Found"
            writeHeaderResponse sslStream PermanentFailure None (Some errorMessage)
            FileDoesntExistError errorMessage

    let returnResponse (sslStream: SslStream) (configuration: ServerConfiguration) (message: string) =
        match message with
        | _message ->
            try
                let indexFilename = Path.Combine(configuration.StaticDirectory, configuration.IndexFile)

                match _message with
                | _ when Uri(_message).LocalPath = "/" && File.Exists(indexFilename) ->
                    writeHeaderResponse sslStream StatusCode.Success (Some "text/gemini") None
                    writeBodyResponse sslStream indexFilename
                    
                    Success (getStatusCode StatusCode.Success, indexFilename)
                | _ ->
                    match getFile (Uri(_message).Segments |> getPathsFromUri) configuration with
                    | Ok file ->
                        nonIndexPageResponse sslStream file
                    | Error err ->
                        writeHeaderResponse sslStream PermanentFailure None (Some "Path Not Found")
                        PathDoesntExistError err
                    
            with
            | :? UnauthorizedAccessException as exn ->
                writeHeaderResponse sslStream PermanentFailure None (Some $"Forbidden access. {exn.Message}")
                UnauthorizedAccessError exn
            | :? UriFormatException as exn ->
                writeHeaderResponse sslStream PermanentFailure None (Some $"URI error. {exn.Message}")
                UriFormatError exn


    let logger = LoggerConfiguration().WriteTo.Console().CreateLogger()

    let parseRequest (sslStream: SslStream) =
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

    let getClientIPAddress (client: TcpClient) =
        let endpoint = client.Client.RemoteEndPoint :?> IPEndPoint
        endpoint.Address

    let handleClient (serverCertificate: X509Certificate2) (configuration: ServerConfiguration) (client: TcpClient) =
        let sslStream = new SslStream(client.GetStream(), false)

        sslStream.AuthenticateAsServer(serverCertificate, false, true)
        sslStream.ReadTimeout <- configuration.RequestTimeoutDuration
        sslStream.WriteTimeout <- configuration.ResponseTimeoutDuration

        logger.Information($"A client with IP address {getClientIPAddress client} connected..")
        let message = parseRequest sslStream
        logger.Information($"A client requested the URI: {message}")

        match returnResponse sslStream configuration message with
        | Success (code, page) ->
            logger.Information($"Successful response to {page} with {code} as status code")
        | FileDoesntExistError err -> logger.Error(err)        
        | PathDoesntExistError err -> logger.Error(err.Message)
        | UnauthorizedAccessError err -> logger.Error(err.Message)
        | UriFormatError err -> logger.Error(err.Message)

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
                handleClient serverCertificate configuration client

                (run state)

            (run true)
        with
        | :? CryptographicException as exn ->
            logger.Error($"SSL certificate validation error! Error: {exn.Message}")
            Environment.Exit(1)
