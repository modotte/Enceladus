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
    
let translatePath (segments: string array) =
    let removeTrailingSlash (str: string) =
        str.TrimEnd([|'/'|])
        
    if Array.length segments = 2 then
        Array.last segments |> removeTrailingSlash
    else
        let path = Array.skip 0 segments |> String.Concat
        path.[1..] |> removeTrailingSlash


let writeHeaderResponse (sslStream: SslStream) (statusCode: StatusCode) (mime: string) =
    match statusCode with
    | TemporaryFailure
    | PermanentFailure
    | ClientCertificateRequired ->
        sslStream.Write(Encoding.UTF8.GetBytes($"{getStatusCode statusCode} An error occured\r\n"))
    | _ -> sslStream.Write(Encoding.UTF8.GetBytes($"{getStatusCode statusCode} {mime}; \r\n"))

let writeBodyResponse (sslStream: SslStream) (text: string) =
    sslStream.Write(Encoding.UTF8.GetBytes($"{text}"))

let returnResponse messageData staticDirectory sslStream =
    match messageData with
    | message ->
        try
            let indexFilename = $"{staticDirectory}/index.gmi"

            match message with
            | _ when Uri(message).LocalPath = "/" && File.Exists(indexFilename) ->
                writeHeaderResponse sslStream StatusCode.Success "text/gemini"
                writeBodyResponse sslStream (File.ReadAllText(indexFilename))
                
                ClientHandlingResult.Success (getStatusCode StatusCode.Success, indexFilename)
            | _ ->
                match getFile (Uri(message).Segments |> translatePath) staticDirectory with
                | Some file -> 
                    let extension, mime = getMIMETypeFromExtension file
                    let filename = $"{staticDirectory}/{Uri(message).Segments |> translatePath}{extension}"
                    
                    writeHeaderResponse sslStream StatusCode.Success mime
                    writeBodyResponse sslStream (File.ReadAllText(filename))
                    
                    ClientHandlingResult.Success (getStatusCode StatusCode.Success, filename)
                | _ ->
                    writeHeaderResponse sslStream StatusCode.PermanentFailure ""
                    PathDoesntExistError $"{Uri(message).AbsolutePath} path does not exist!"
                
        with
        | :? IOException as ex -> IOError ex
        | :? AuthenticationException as ex -> AuthenticationError ex

[<Literal>]
let MAX_BUFFER_LENGTH = 1048

let logger =
    LoggerConfiguration().WriteTo.Console().CreateLogger()

let readClientRequest (stream: SslStream) =
    let mutable buffer = Array.zeroCreate MAX_BUFFER_LENGTH
    let messageData = StringBuilder()
    let mutable bytes = -1

    // TODO: Supports image files (png and jpg).
    // Currently, the server ended up parsing them into application/octet-stream only?
    while bytes <> 0 do
        bytes <- stream.Read(buffer, 0, buffer.Length)
        let decoder = Encoding.UTF8.GetDecoder()

        let mutable chars =
            Array.zeroCreate (decoder.GetCharCount(buffer, 0, bytes))

        decoder.GetChars(buffer, 0, bytes, chars, 0) |> ignore

        messageData.Append(chars) |> ignore

        match messageData.ToString().IndexOf("\r\n") with
        | bytesCount when bytesCount <> -1 -> bytes <- 0
        | _ -> ()

    messageData.ToString()

let handleClient (client: TcpClient) (serverCertificate: X509Certificate2) (staticDirectory: string) =
    let sslStream = new SslStream(client.GetStream(), false)

    let timeoutDuration = 5000
    sslStream.AuthenticateAsServer(serverCertificate, false, true)
    sslStream.ReadTimeout <- timeoutDuration
    sslStream.WriteTimeout <- timeoutDuration

    logger.Information("A client connected..")
    let messageData = readClientRequest sslStream
    logger.Information("A client requested some resources..")

    match returnResponse messageData staticDirectory sslStream with
    | ClientHandlingResult.Success (code, page) when
        code >= getStatusCode StatusCode.Success
        && code <= getStatusCode Redirect
        ->
        logger.Information($"Successful response to {page} with {code} as status code")
    | IOError err -> logger.Error(err.Message)
    | PathDoesntExistError err -> logger.Error(err)
    | AuthenticationError err -> logger.Error(err.Message)
    | _ -> logger.Error("An unknown error occured")

    sslStream.Close()
    client.Close()

    logger.Information("Closed last client connection..")

let runServer (host: string) port staticDirectory (serverCertificate: X509Certificate2) =
    let hostInfo = Dns.GetHostEntry(host)
    let ipAddress = hostInfo.AddressList.[0]
    let listener = TcpListener(ipAddress, port)
    listener.Start()

    while true do
        logger.Information($"Waiting for a client to connect at gemini://{host}:{port}/")
        let client = listener.AcceptTcpClient()
        handleClient client serverCertificate staticDirectory

type ConfigType = IniFile<"config.ini">
[<EntryPoint>]
let main _ =

    runServer
        ConfigType.Server.host
        ConfigType.Server.port
        ConfigType.Server.staticDirectory
        (new X509Certificate2(
            ConfigType.Server.certificateFilePFXPath,
            string ConfigType.Server.certificatePassword))

    0
