open System
open System.Net.Security
open System.Security.Cryptography.X509Certificates
open System.Net
open System.Net.Sockets
open System.Text

type StatusCode =
    | Input | Success | Redirect
    | TemporaryFailure | PermanentFailure | ClientCertificateRequired


type Server() =
    let port = 1965
    let mutable serverCertificate = null
    
    member this.DisplaySecurityLevel(stream: SslStream) =
            printfn $"Cipher: {stream.CipherAlgorithm} strength {stream.CipherStrength}"
            printfn $"Hash: {stream.HashAlgorithm} strength {stream.HashStrength}"
            printfn $"Key exchange: {stream.KeyExchangeAlgorithm} strength {stream.KeyExchangeStrength}"
            printfn $"Protocol: {stream.SslProtocol}"
            
    member this.HandleClient(client: TcpClient) =
        let sslStream = new SslStream(client.GetStream(), false)
        try
            let timeoutDuration = 5000
            sslStream.AuthenticateAsServer(serverCertificate, false, true)
            this.DisplaySecurityLevel(sslStream)
            sslStream.ReadTimeout <- timeoutDuration
            sslStream.WriteTimeout <- timeoutDuration
            
            let headerResponse = Encoding.UTF8.GetBytes("20 text/gemini; charset=utf8 \r\n")
            sslStream.Write(headerResponse)
            
            sslStream.Write(Encoding.UTF8.GetBytes("hello! 你好 \r\n"))
            sslStream.Write(Encoding.UTF8.GetBytes("=> https://google.com Google Search Engine \r\n"))
            
            sslStream.Close()
            client.Close()
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