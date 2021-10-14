# Enceladus

A simple, synchronous Gemini protocol compliant server implementation in .NET and F#.

## What is Gemini network protocol?

Gemini is a new application-level internet protocol for the distribution of arbitrary files, with some special consideration for serving a lightweight hypertext format which facilitates linking between files. You may think of Gemini as "the web, stripped right back to its essence" or as "Gopher, souped up and modernised just a little", depending upon your perspective (the latter view is probably more accurate). Gemini may be of interest to people who are:

* Opposed to the web's ubiquitous tracking of users
* Tired of nagging pop-ups, obnoxious adverts, autoplaying videos and other misfeatures of the modern web
* Interested in low-power computing and/or low-speed networks, either by choice or necessity

For more details, please visit [Gemini FAQ](https://gemini.circumlunar.space/docs/faq.gmi)

## Supported Platforms

Theoretically, Enceladus could be run on Windows and MacOSX without any further modifications,
but currently, this server has been tested to run well on Linux system so far.

- Linux: Supported
- Windows: In the future
- MacOSX: In the future

## Status

Experimental

## What's next?

[ ] Redesign for concurrent and simultaneous client connections.
[ ] Improve error handling model.
[ ] Support for client certificate validation.

## Running

> NOTE: You can override server properties in `config.json`.

1. Generate SSL certificates first by running `sh generate_ssl.sh CERT_FILE`.
2. Change server credentials according to your setup in `config.ini` file.
3. `dotnet run` or `./Enceladus` to start the server.
4. Visit `gemini://localhost:1965/`, the homepage of the server using the client that you can find by scrolling to the bottom of this page.

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License

This software is licensed under the MIT license. For more details,
please see LICENSE file.
