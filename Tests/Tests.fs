module Tests

open NUnit.Framework
open FsUnit
open Enceladus.Core

[<Test>]
let GetMimeTypeFromExtensionTest () =
    extractMIMEFromExtension "malkin.html" |> should equal "text/html"
    extractMIMEFromExtension "M723Jhhs.jpg" |> should equal "image/jpeg"
    extractMIMEFromExtension "meow.gmi" |> should equal "text/gemini"
    extractMIMEFromExtension "82yqoiAWJKS.txt" |> should equal "text/plain"
    extractMIMEFromExtension "jhawhsha.jpeg" |> should equal "image/jpeg"
    extractMIMEFromExtension "8wjs0002j020hahsiowa.png" |> should equal "image/png"    

[<Test>]
let GetPathsFromUriTest () =
    makePathsFromUri [|"/"; "/owo/"|] |> should equal ("", "owo")
    makePathsFromUri [|"/"; "/about"; "/meow"|] |> should equal ("about", "meow")
    makePathsFromUri [|"/"; "/rowo"|] |> should equal ("", "rowo")
    makePathsFromUri [|"/"; "/subdir"; "/code"; "c"|] |> should equal ("subdir/code", "c")

