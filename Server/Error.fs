namespace Enceladus

open System.IO
open System.Security.Authentication

module Error =
    type ClientError =
    | FileDoesntExistError of string
    | PathDoesntExistError of DirectoryNotFoundException
    | AuthenticationError of AuthenticationException