module Tests

open NUnit.Framework
open FsUnit
open Enceladus.Core

[<Test>]
let GetMimeTypeFromExtensionTest () =
    getMIMETypeFromExtension "malkin.html" |> should equal "text/html"
    getMIMETypeFromExtension "M723Jhhs.jpg" |> should equal "image/jpeg"
    getMIMETypeFromExtension "meow.gmi" |> should equal "text/gemini"
    getMIMETypeFromExtension "82yqoiAWJKS.txt" |> should equal "text/plain"
    getMIMETypeFromExtension "megacol" |> should equal "text/plain"
    getMIMETypeFromExtension "jhawhsha.jpeg" |> should equal "image/jpeg"
    getMIMETypeFromExtension "8wjs0002j020hahsiowa.png" |> should equal "image/png"    

[<Test>]
let RefinePathTest () =
    refinePath [|"/"; "/owo/"|] |> should equal ("", "owo")
    refinePath [|"/"; "/about"; "/meow"|] |> should equal ("about", "meow")
    refinePath [|"/"; "/rowo"|] |> should equal ("", "rowo")
    refinePath [|"/"; "/subdir"; "/code"; "c"|] |> should equal ("subdir/code", "c")

