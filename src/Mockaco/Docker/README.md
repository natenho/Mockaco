# Quick reference

-	**Where to get help and to file issues**:  
	[GitHub repository](https://github.com/natenho/Mockaco/)

-	**Maintained by**:  
	[natenho](https://github.com/natenho)

# What is Mockaco?

Mockaco is an HTTP-based API mock server with fast setup, featuring:

- Simple JSON-based configuration
- Pure C# scripting - you don't need to learn a new specific language or API to configure your mocks
- Fake data generation - built-in hassle-free fake data generation
- Callback support - trigger another service call when a request hits your mocked API

<img src="https://image.flaticon.com/icons/svg/1574/1574279.svg" width="100px" height="100px" alt="logo">

# How to use this image

## Running the demo

The default image ships with a sample "hello" mock:

```console
$ docker run -it --rm -p 5000:5000 natenho/mockaco
```

Mockaco can be accessed by any HTTP client via `http://localhost:5000`

```console
$ curl -iX GET http://localhost:5000/hello/docker
```
```http
HTTP/1.1 200 OK
Date: Wed, 21 Jun 2019 05:10:00 GMT
Content-Type: application/json
Server: Kestrel
Transfer-Encoding: chunked

{
	"message": "Hello docker!"
}
```

## Running and creating your own mocks

The best way to use the image is by creating a directory on the host system (outside the container) and mount this to the `/app/Mocks` directory inside the container.

1. Create a data directory on a suitable volume on your host system, e.g. `/my/own/mockdir`.
2. Start your `mockaco` container like this:

```console
$ docker run -it --rm -p 5000:80 -v /my/own/mockdir:/app/Mocks natenho/mockaco
```

The `-v /my/own/mockdir:/app/Mocks` part of the command mounts the `/my/own/mockdir` directory from the underlying host system as `/app/Mocks` inside the container, where Mockaco by default will read its mock JSON files.

3. Create a request/response template file named `PingPong.json` under `/my/own/mockdir` folder:

```json
{
  "request": {
	"method": "GET",
	"route": "ping"
  },
  "response": {
	"status": "OK",
	"body": {
	  "response": "pong"
	}
  }
}
```

4. Send a request and get the mocked response, running:

```console
$ curl -iX GET http://localhost:5000/ping
```
```http
HTTP/1.1 200 OK
Date: Wed, 21 Jun 2019 05:10:00 GMT
Content-Type: application/json
Server: Kestrel
Transfer-Encoding: chunked

{
	"response": "pong"
}
```

For advanced usage scenarios, like scripting and fake data generation, refer to the [docs](https://github.com/natenho/Mockaco).