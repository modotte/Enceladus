# Enceladus

A synchronous Gemini protocol compliant server implementation in .NET and F#.
Currently, serves as a static file server on Linux.

## Features

* Make use *index.gmi* file as fixed default homepage.
* Handles Gemini, JSON and HTML files. Others will be treated as plain text.
* Handle subdirectories.

## Status

Experimental.

## Running

1. Generate SSL certificates first by running `sh generate_ssl.sh CERT_FILE`.
2. Change server credentials according to your setup in `config.ini` file.
3. `dotnet run` or `./Enceladus` to start the server.

## License

This software is licensed under the MIT license. For more details,
please see LICENSE file.
