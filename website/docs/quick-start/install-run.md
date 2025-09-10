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
$ docker run --pull always -it --rm -p 5000:5000 -v /your/folder:/app/Mocks natenho/mockaco
```

### Docker command breakdown
Flag | Description | Purpose |
:--- | :--- | :--- |
`docker run` | The foundational command | Creates and starts a new Docker container. |
`--pull always` | Always pull image | Ensures you're running the latest version from the registry. |
`-it` | Interactive TTY | Attaches your terminal to the container to see logs and use `Ctrl+C`. |
`--rm` | Remove on exit | Automatically cleans up and deletes the container when it stops. |
`-p 5000:5000` | Publish port | Maps `localhost:5000` on your machine to port `5000` in the container. The port exposed by the container is 5000 (HTTP) by default. |
`-v /your/folder:/app/Mocks` | Mount volume | Links your local folder to the container so `mockaco` can read your mock files. |
`natenho/mockaco` | The Docker image | The actual application being run—the mock server itself. |

## From sources

Or your can run it directly from sources:

```console
$ git clone https://github.com/natenho/Mockaco.git
$ cd Mockaco\src\Mockaco
$ dotnet run
```

A random local port is chosen if `--urls` parameter is not provided.
