# Enceladus

A synchronous Gemini protocol compliant server implementation in .NET and F#.
Currently, serves as a static file server on Linux.

## Features

* Make use *index.gmi* file as fixed default homepage.
* Handles Gemini, plain text, JSON and HTML files.

## Status

Experimental.

## Running

> Generate SSL certificates first by running `sh generate_ssl.sh`.

`dotnet run -- <CERT_FILE.pfx> <PASSWORD>` to start the server.

## License

This software is licensed under the MIT license. For more details,
please see LICENSE file.
