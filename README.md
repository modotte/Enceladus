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

v0.1.0

## What's next?

- [ ] Redesign for concurrent and simultaneous client connections.
- [ ] Improve error handling model.
- [ ] Support for client certificate validation.

## Prerequisites to build

* [dotnet](https://dotnet.microsoft.com/download) (>= v5.0.0)
* openssl

### Running

> NOTE: You can override server properties in `config.json`.

1. Change current directory into ***Server*** directory.
2. Generate SSL certificates first by running `sh generate_ssl.sh`.
3. Change server credentials according to your setup in `config.json` file.
    - You can override ***config.json*** location by assigning the absolute path using `ENCELADUS_CONFIG_FILE` environment variable.
    - Example: `ENCELADUS_CONFIG_FILE="$HOME/config.json"` on Linux systems.
4. `dotnet run` or `./Enceladus` to start the server.
5. Visit `gemini://localhost:1965/`, the homepage of the server using the client that you can find by scrolling to the bottom of this page.

## How to contribute
Imposter syndrome disclaimer: I want your help. No really, I do.

There might be a little voice inside that tells you you're not ready; that you need to do one more tutorial, or learn another framework, or write a few more blog posts before you can help me with this project.

I assure you, that's not the case.

This project has some clear Contribution Guidelines and expectations that you can [read here](https://github.com/modotte/Enceladus/blob/master/CONTRIBUTING.md).

The contribution guidelines outline the process that you'll need to follow to get a patch merged. By making expectations and process explicit, I hope it will make it easier for you to contribute.

And you don't just have to write code. You can help out by writing documentation, tests, or even by giving feedback about this work.

Thank you for contributing!

## License

This software is licensed under the MIT license. For more details,
please see LICENSE file.

## See also

### Gemini ciients (aka browsers)

- [Amfora](https://github.com/makeworld-the-better-one/amfora#amfora), the primary client that I use to test the server.
- [Castor](https://git.sr.ht/~julienxx/castor)

### More

- [Awesome Gemini](https://github.com/kr1sp1n/awesome-gemini#readme)

