# Enceladus

A synchronous Gemini protocol compliant server implementation in .NET and F#.
Currently, serves as a static file server on Linux.

## Features

* Make use *index.gmi* file as fixed default homepage.
* Handles Gemini, Markdown, and HTML MIME types. Others will be treated as plain text.
* Handle subdirectories.

## Status

Experimental.

## Running

1. Change directory into *Server* directory
2. Generate SSL certificates first by running `sh generate_ssl.sh CERT_FILE`.
3. Change server credentials according to your setup in `config.ini` file.
4. `dotnet run` or `./Enceladus` to start the server.

## Testing

Run `dotnet test` to execute the unit tests.

## License

This software is licensed under the MIT license. For more details,
please see LICENSE file.
