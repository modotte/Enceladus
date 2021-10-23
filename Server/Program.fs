namespace Enceladus

open System
open System.IO
open FSharp.Json
open Enceladus.Server

module Program =
    [<EntryPoint>]
    let main _ =
        let configurationFile = Environment.GetEnvironmentVariable("ENCELADUS_CONFIG_FILE")
        
        try
            if configurationFile |> String.IsNullOrEmpty then
                File.ReadAllText("config.json")
            else
                File.ReadAllText(configurationFile)
            |> Json.deserialize<ServerConfiguration>
            |> runServer
        with
        | :? FileNotFoundException as exn ->
            logger.Error(exn.Message)
            Environment.Exit(1)
        
        0
