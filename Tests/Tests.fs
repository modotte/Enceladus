module Tests

open NUnit.Framework
open FsUnit
open Enceladus.Core

[<Test>]
let ExtractMimeFromExtensionTest () =
    extractMIMEFromExtension "malkin.html" |> should equal "text/html"
    extractMIMEFromExtension "M723Jhhs.jpg" |> should equal "image/jpeg"
    extractMIMEFromExtension "meow.gmi" |> should equal "text/gemini"
    extractMIMEFromExtension "82yqoiAWJKS.txt" |> should equal "text/plain"
    extractMIMEFromExtension "jhawhsha.jpeg" |> should equal "image/jpeg"
    extractMIMEFromExtension "8wjs0002j020hahsiowa.png" |> should equal "image/png"    

[<Test>]
let CombinePathsFromUriTest () =
    combinePathsFromUri [|"/"; "/owo/"|] |> should equal ("", "owo")
    combinePathsFromUri [|"/"; "/about"; "/meow"|] |> should equal ("about", "meow")
    combinePathsFromUri [|"/"; "/rowo"|] |> should equal ("", "rowo")
    combinePathsFromUri [|"/"; "/subdir"; "/code"; "c"|] 
    |> should equal 
    #if _WINDOWS
        ("subdir\\code", "c")
    #else
        ("subdir/code", "c")
    #endif        
    combinePathsFromUri [|"/"; "/subdirm/"; "/metao84"; "/capabi923jhw"; "w"|] 
    |> should equal 
    #if _WINDOWS
        ("subdirm\\metao84\\capabi923jhw", "w")
    #else
        ("subdirm/metao84/capabi923jhw", "w")
    #endif

