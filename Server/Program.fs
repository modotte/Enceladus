namespace Enceladus

open System
open System.Net.Security
open System.Security.Authentication
open System.Security.Cryptography.X509Certificates
open System.Net
open System.Net.Sockets
open System.IO
open System.Text
open Serilog
open FSharp.Configuration

module Server =
    type StatusCode =
        | Input
        | Success
        | Redirect
        | TemporaryFailure
        | PermanentFailure
        | ClientCertificateRequired

    type ClientHandlingResult =
        | Success of int * string
        | IOError of IOException
        | PathDoesntExistError of string
        | AuthenticationError of AuthenticationException

    let getStatusCode =
        function
        | Input -> 10
        | StatusCode.Success -> 20
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
        
    let getFile (filename: string) (directory: string) =
        Directory.GetFiles(directory, $"{filename}.?*", SearchOption.AllDirectories) |> Array.tryHead
        
    let refinePath (pathSegments: string array) =
        let removeTrailingSlash (str: string) =
            str.TrimEnd([|'/'|])
            
        if Array.length pathSegments = 2 then
            Array.last pathSegments |> removeTrailingSlash
        else
            let path = Array.skip 0 pathSegments |> String.Concat
            path.[1..] |> removeTrailingSlash


    let writeHeaderResponse (sslStream: SslStream) (statusCode: StatusCode) (mime: string) =
        match statusCode with
        | TemporaryFailure
        | PermanentFailure
        | ClientCertificateRequired ->
            sslStream.Write(Encoding.UTF8.GetBytes($"{getStatusCode statusCode} An error occured\r\n"))
        | _ -> sslStream.Write(Encoding.UTF8.GetBytes($"{getStatusCode statusCode} {mime}; \r\n"))

    let writeBodyResponse (sslStream: SslStream) (text: string) = sslStream.Write(Encoding.UTF8.GetBytes(text))

    let returnResponse (sslStream: SslStream) (message: string) (staticDirectory: string) =
        match message with
        | _message ->
            try
                let indexFilename = $"{staticDirectory}/index.gmi"

                match _message with
                | _ when Uri(_message).LocalPath = "/" && File.Exists(indexFilename) ->
                    writeHeaderResponse sslStream StatusCode.Success "text/gemini"
                    writeBodyResponse sslStream (File.ReadAllText(indexFilename))
                    
                    ClientHandlingResult.Success (getStatusCode StatusCode.Success, indexFilename)
                | _ ->
                    match getFile (Uri(_message).Segments |> refinePath) staticDirectory with
                    | Some file -> 
                        let extension, mime = getMIMETypeFromExtension file
                        let filename = $"{staticDirectory}/{Uri(_message).Segments |> refinePath}{extension}"
                        
                        writeHeaderResponse sslStream StatusCode.Success mime
                        writeBodyResponse sslStream (File.ReadAllText(filename))
                        
                        ClientHandlingResult.Success (getStatusCode StatusCode.Success, filename)
                    | _ ->
                        writeHeaderResponse sslStream StatusCode.PermanentFailure ""
                        PathDoesntExistError $"{Uri(_message).AbsolutePath} path does not exist!"
                    
            with
            | :? IOException as exn -> IOError exn
            | :? AuthenticationException as exn -> AuthenticationError exn

    [<Literal>]
    let MAX_BUFFER_LENGTH = 1048
    let logger = LoggerConfiguration().WriteTo.Console().CreateLogger()

    let readClientRequest (sslStream: SslStream) =
        // TODO: Investigate this buffer logic reasoning.
        let mutable buffer = Array.zeroCreate MAX_BUFFER_LENGTH
        let message = StringBuilder()
        let mutable bytes = -1

        // TODO: Supports image files (png and jpg).
        // Currently, the server ended up parsing them into application/octet-stream only?
        while bytes <> 0 do
            bytes <- sslStream.Read(buffer, 0, buffer.Length)
            let decoder = Encoding.UTF8.GetDecoder()
            let mutable characters =
                Array.zeroCreate (decoder.GetCharCount(buffer, 0, bytes))
            decoder.GetChars(buffer, 0, bytes, characters, 0) |> ignore
            message.Append(characters) |> ignore

            match message.ToString().IndexOf("\r\n") with
            | bytesCount when bytesCount <> -1 -> bytes <- 0
            | _ -> ()

        message.ToString()

    let handleClient (serverCertificate: X509Certificate2) (client: TcpClient) (staticDirectory: string) =
        let sslStream = new SslStream(client.GetStream(), false)

        // TODO: Put `timeoutDuration` inside config.ini
        let timeoutDuration = 5000
        sslStream.AuthenticateAsServer(serverCertificate, false, true)
        sslStream.ReadTimeout <- timeoutDuration
        sslStream.WriteTimeout <- timeoutDuration

        logger.Information("A client connected..")
        let message = readClientRequest sslStream
        logger.Information("A client requested some resources..")

        // BUG: Fix unhandled error when retrieving unknown path on an existing base file in URI
        // Example: gemini://localhost/about/notExist/noteventhispath
        match returnResponse sslStream message staticDirectory with
        | ClientHandlingResult.Success (code, page) when
            code >= getStatusCode StatusCode.Success
            && code <= getStatusCode Redirect ->
                logger.Information($"Successful response to {page} with {code} as status code")
        | IOError err -> logger.Error(err.Message)
        | PathDoesntExistError err -> logger.Error(err)
        | AuthenticationError err -> logger.Error(err.Message)
        | _ -> logger.Error("An unknown has error occured")

        sslStream.Close()
        client.Close()

        logger.Information("Closed last client connection..")

    let runServer (serverCertificate: X509Certificate2) (host: string) (port: int) (staticDirectory: string) =
        let hostInfo = Dns.GetHostEntry(host)
        let ipAddress = hostInfo.AddressList.[0]
        let listener = TcpListener(ipAddress, port)
        listener.Start()

        while true do
            logger.Information($"Waiting for a client to connect at gemini://{host}:{port}/")
            let client = listener.AcceptTcpClient()
            handleClient serverCertificate client staticDirectory

    type ConfigType = IniFile<"config.ini">
    [<EntryPoint>]
    let main _ =
        runServer
            (new X509Certificate2(
                ConfigType.Server.certificateFilePFXPath,
                string ConfigType.Server.certificatePassword))
            ConfigType.Server.host
            ConfigType.Server.port
            ConfigType.Server.staticDirectory

        0
