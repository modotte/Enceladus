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

### Version

v0.1.1

## What's next?

- [ ] Redesign for concurrent and simultaneous client connections.
- [ ] Improve error handling model.
- [ ] Support for client certificate validation.

## Prerequisites to build

* [dotnet](https://dotnet.microsoft.com/download) (>= v5.0.0)
* openssl

### Running

> NOTE: You can override server properties in `config.json`.

1. Install dependencies
   - Run:
   ```sh
   dotnet tool restore
   dotnet paket restore
   
   # or if you're on Linux or Mac
   bash ./build.sh
   ```
2. Change current directory into ***Server*** directory.
3. Generate SSL certificates first by running `sh generate_ssl.sh`.
4. Change server credentials according to your setup in `config.json` file.
    - You can override ***config.json*** location by assigning the absolute path using `ENCELADUS_CONFIG_FILE` environment variable.
    - Example: `ENCELADUS_CONFIG_FILE="$HOME/config.json"` on Linux systems.
5. `dotnet run` or `./Enceladus` to start the server.
6. Visit `gemini://localhost:1965/`, the homepage of the server using the client that you can find by scrolling to the bottom of this page.

## How to contribute
Please read CONTRIBUTING.md first.

Thank you for contributing! :smile:

## License

This software is licensed under the MIT license. For more details,
please see LICENSE file.

## See also

### Gemini clients (aka browsers)

- [Amfora](https://github.com/makeworld-the-better-one/amfora#amfora), the primary client that I use to test the server.
- [Castor](https://git.sr.ht/~julienxx/castor)

### More

- [Awesome Gemini](https://github.com/kr1sp1n/awesome-gemini#readme)

