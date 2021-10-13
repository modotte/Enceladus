module Tests

open NUnit.Framework
open FsUnit
open Enceladus.Core

[<SetUp>]
let Setup () =
    ()

[<Test>]
let RefinePathTest () =
    refinePath [|"/"; "/owo/"|] |> should equal "/owo"
    refinePath [|"/"; "/about"; "/meow"|] |> should equal "/about/meow"
    refinePath [|"/"; "/rowo"|] |> should equal "/rowo"

[<Test>]
let GetMimeTypeFromExtensionTest () =
    getMIMETypeFromExtension "malkin.html" |> should equal (".html", "text/html")
    getMIMETypeFromExtension "M723Jhhs.jpg" |> should equal (".jpg", "image/jpeg")
    getMIMETypeFromExtension "meow.gmi" |> should equal (".gmi", "text/gemini")
    getMIMETypeFromExtension "82yqoiAWJKS.txt" |> should equal (".txt", "text/plain")
    getMIMETypeFromExtension "megacol" |> should equal ("", "text/plain")
    getMIMETypeFromExtension "jhawhsha.jpeg" |> should equal (".jpeg", "image/jpeg")
    getMIMETypeFromExtension "8wjs0002j020hahsiowa.png" |> should equal (".png", "image/png")