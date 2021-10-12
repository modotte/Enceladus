namespace Enceladus

open System.IO
open System.Security.Authentication
open System.Net.Sockets

module Error =
    type ServerError =
    | FileDoesntExistError
    | PathDoesntExistError of DirectoryNotFoundException
    | AuthenticationError of AuthenticationException
    | AddressAlreadyInUseError of SocketException

    type ConfigurationError =
    | FileDoesntExistError
    | PropertyDoesntExistError