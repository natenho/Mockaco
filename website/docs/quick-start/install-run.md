---
sidebar_position: 1
---

# Install and run

Choose your favorite way to install Mockaco locally or in your server.

## .NET CLI

Install and run as a dotnet tool:

```console
$ dotnet tool install -g mockaco
$ mockaco --urls "http://localhost:5000"
```

A random local port is chosen if `--urls` parameter is not provided.

## Docker

You can run Mockaco from the official [Docker image](https://hub.docker.com/r/natenho/mockaco) (replace ```/your/folder``` with an existing directory of your preference):

```console
$ docker run -it --rm -p 5000:5000 -v /your/folder:/app/Mocks natenho/mockaco
```

The port exposed by the container is 5000 (HTTP) by default.

## From sources

Or your can run it directly from sources:

```console
$ git clone https://github.com/natenho/Mockaco.git
$ cd Mockaco\src\Mockaco
$ dotnet run
```

A random local port is chosen if `--urls` parameter is not provided.
