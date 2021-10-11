# Enceladus

A synchronous Gemini protocol compliant server implementation in .NET and F#.
Currently, serves as a static file server on Linux.

## Features

* Make use *index.gmi* file as fixed default homepage.
* Handles Gemini, Markdown, and HTML MIME types. Others will be treated as plain text.
* Handle subdirectories.

## Status

Experimental (on Linux only)

## Prerequisites to build

* [dotnet](https://dotnet.microsoft.com/download) (>= v5.0.0)
* openssl

### Running

1. Generate SSL certificates first by running `sh generate_ssl.sh CERT_FILE`.
2. Change server credentials according to your setup in `config.json` file.
    - You can override ***config.json*** location by assigning the absolute path using `ENCELADUS_CONFIG_FILE` environment variable.
    - Example: `ENCELADUS_CONFIG_FILE="$HOME/config.json"` on Linux systems.
3. `dotnet run` or `./Enceladus` to start the server.
4. Visit `gemini://localhost:1965/` to visit the homepage of the server using the clients that you can find by scrolling to the bottom of this page.

## License

This software is licensed under the MIT license. For more details,
please see LICENSE file.

## See Also

### Gemini ciients (aka browsers)

- [Amfora](https://github.com/makeworld-the-better-one/amfora#amfora), the primary client that I use to test the server.
- [Castor](https://git.sr.ht/~julienxx/castor)
