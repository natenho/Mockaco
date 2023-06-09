<h1>
    <img src="https://github.com/natenho/Mockaco/raw/master/src/Mockaco/Resources/mockaco-logo.svg" width="64px" height="64px" alt="Mockaco">
    Mockaco 
</h1>

Mockaco is an HTTP-based API mock server with fast setup.

[![Main Build](https://github.com/natenho/Mockaco/actions/workflows/main-release.yml/badge.svg)](https://github.com/natenho/Mockaco/actions/workflows/main-release.yml) [![Docker Pulls](https://img.shields.io/docker/pulls/natenho/mockaco)](https://hub.docker.com/repository/docker/natenho/mockaco) [![Nuget](https://img.shields.io/nuget/dt/Mockaco?color=blue&label=nuget%20downloads)](https://www.nuget.org/packages/Mockaco/) [![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fnatenho%2FMockaco.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2Fnatenho%2FMockaco?ref=badge_shield)

## Documentation

Access the docs on [natenho.github.io/Mockaco](https://natenho.github.io/Mockaco/)

## Features

- Simple JSON-based configuration
- Pure C# scripting - you don't need to learn a new specific language or API to configure your mocks
- Fake data generation - built-in fake data generation
- Callback support - trigger another service call when a request hits your mocked API
- State support - stateful mock support allow a mock to be returned based on a global variable previously set by another mock
- Portable - runs in any [.NET Core compatible environment](https://github.com/dotnet/core/blob/main/release-notes/5.0/5.0-supported-os.md)

[![Mocking APIs with Mockaco | .NET 7](https://user-images.githubusercontent.com/4236481/195997781-b730959e-8d6d-432c-b35a-3adb580abc41.png)](https://www.youtube.com/watch?v=QBnXCgZFzM0 "Mocking APIs with Mockaco | .NET 7")

# Get Started

## Running the application

### .NET CLI

Install and run as a dotnet tool:

```console
$ dotnet tool install -g mockaco
$ mockaco --urls "http://localhost:5000"
```

A random local port is chosen if `--urls` parameter is not provided.

### Docker

You can run Mockaco from the official [Docker image](https://hub.docker.com/r/natenho/mockaco) (replace ```/your/folder``` with an existing directory of your preference):

```console
$ docker run -it --rm -p 5000:5000 -v /your/folder:/app/Mocks natenho/mockaco
```

The port exposed by the container is 5000 (HTTP) by default.

### Sources

Or your can run it directly from sources:

```console
$ git clone https://github.com/natenho/Mockaco.git
$ cd Mockaco\src\Mockaco
$ dotnet run
```

A random local port is chosen if `--urls` parameter is not provided.

--

Icon made by [Freepik](https://www.freepik.com/ "Freepik") from [www.flaticon.com](https://www.flaticon.com/ "Flaticon") is licensed by [CC 3.0 BY](http://creativecommons.org/licenses/by/3.0/ "Creative Commons BY 3.0")


## License
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fnatenho%2FMockaco.svg?type=large)](https://app.fossa.com/projects/git%2Bgithub.com%2Fnatenho%2FMockaco?ref=badge_large)
