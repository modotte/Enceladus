open System
open System.Net.Security
open System.Security.Cryptography.X509Certificates
open System.Net
open System.Net.Sockets
open System.Text

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
    
    member this.DisplaySecurityLevel(stream: SslStream) =
            printfn $"Cipher: {stream.CipherAlgorithm} strength {stream.CipherStrength}"
            printfn $"Hash: {stream.HashAlgorithm} strength {stream.HashStrength}"
            printfn $"Key exchange: {stream.KeyExchangeAlgorithm} strength {stream.KeyExchangeStrength}"
            printfn $"Protocol: {stream.SslProtocol}"
            
     
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
                
        messageData.ToString()
                
            
    member this.HandleClient(client: TcpClient) =
        let sslStream = new SslStream(client.GetStream(), false)
        try
            let timeoutDuration = 5000
            sslStream.AuthenticateAsServer(serverCertificate, false, true)
            this.DisplaySecurityLevel(sslStream)
            sslStream.ReadTimeout <- timeoutDuration
            sslStream.WriteTimeout <- timeoutDuration
            
            printfn "Waiting for request URL.."
            let messageData = this.ReadClientRequest(sslStream)
            printfn $"Received: {messageData}"
            
            let headerResponse = Encoding.UTF8.GetBytes($"{getStatusCode Success} text/gemini; charset=utf8 \r\n")
            sslStream.Write(headerResponse)
            
            sslStream.Write(Encoding.UTF8.GetBytes("Hello! 你好 \r\n"))
            sslStream.Write(Encoding.UTF8.GetBytes("=> https://google.com Google Search Engine \r\n"))
            
            sslStream.Close()
            client.Close()
            printfn "Closed old connections"
            
        finally
        sslStream.Close()
        client.Close()
            
    member this.RunServer(certificate: string, certificatePassword: string) =
        serverCertificate <- new X509Certificate2(certificate, certificatePassword)
        let listener = TcpListener(IPAddress.Any, port)
        listener.Start()
        while true do
            printfn $"Waiting for a client to connect at port {port}"
            let client = listener.AcceptTcpClient()
            this.HandleClient(client)
            
       
[<EntryPoint>]
let main argv =
    let server = Server()
    let certificate = argv.[0]
    let certificatePassword = argv.[1]
    server.RunServer(certificate, certificatePassword)
    
    0