# Enceladus

A synchronous Gemini protocol compliant server implementation in .NET and F#.
Currently, serves as a static file server on Linux.

## Status

Experimental (on Linux only)

### Version

v0.1.0

## Prerequisites to build

* [dotnet](https://dotnet.microsoft.com/download) (>= v5.0.0)
* openssl

### Running

1. Change current directory into *Server/* directory.
2. Generate SSL certificates first by running `sh generate_ssl.sh`.
3. Change server credentials according to your setup in `config.json` file.
    - You can override ***config.json*** location by assigning the absolute path using `ENCELADUS_CONFIG_FILE` environment variable.
    - Example: `ENCELADUS_CONFIG_FILE="$HOME/config.json"` on Linux systems.
4. `dotnet run` or `./Enceladus` to start the server.
5. Visit `gemini://localhost:1965/`, the homepage of the server using the clients that you can find by scrolling to the bottom of this page.

## License

This software is licensed under the MIT license. For more details,
please see LICENSE file.

## See Also

### Gemini clients (aka browsers)

- [Amfora](https://github.com/makeworld-the-better-one/amfora#amfora), the primary client that I use to test the server.
- [Castor](https://git.sr.ht/~julienxx/castor)
