module Tests

open NUnit.Framework
open FsUnit
open Enceladus.Core

[<Test>]
let mimeFromExtensionTest () =
    mimeFromExtension "malkin.html" |> should equal "text/html"
    mimeFromExtension "M723Jhhs.jpg" |> should equal "image/jpeg"
    mimeFromExtension "meow.gmi" |> should equal "text/gemini"
    mimeFromExtension "82yqoiAWJKS.txt" |> should equal "text/plain"
    mimeFromExtension "jhawhsha.jpeg" |> should equal "image/jpeg"
    mimeFromExtension "8wjs0002j020hahsiowa.png" |> should equal "image/png"    
