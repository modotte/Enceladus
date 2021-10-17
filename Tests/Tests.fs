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
    getMIMETypeFromExtension "jhawhsha.jpeg" |> should equal "image/jpeg"
    getMIMETypeFromExtension "8wjs0002j020hahsiowa.png" |> should equal "image/png"    

[<Test>]
let GetPathsFromUriTest () =
    getPathsFromUri [|"/"; "/owo/"|] |> should equal ("", "owo")
    getPathsFromUri [|"/"; "/about"; "/meow"|] |> should equal ("about", "meow")
    getPathsFromUri [|"/"; "/rowo"|] |> should equal ("", "rowo")
    getPathsFromUri [|"/"; "/subdir"; "/code"; "c"|] |> should equal ("subdir/code", "c")

