module Tests

open NUnit.Framework
open FsUnit
open Enceladus.Server

[<SetUp>]
let Setup () =
    ()

[<Test>]
let All () =
    refinePath [|"/"; "/owo/"|] |> should equal "/owo"
    refinePath [|"/"; "/about"; "/meow"|] |> should equal "/about/meow"
    refinePath [|"/"; "/rowo"|] |> should equal "/rowo"
