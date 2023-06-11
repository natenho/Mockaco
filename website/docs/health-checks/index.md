# Health Checks

Health checks are often useful when Mockaco container is being used.

## `/_mockaco/health`

To determine if Mockaco is running and listening, you can check its health status by accessing http://localhost:5000/_mockaco/health. A successful request will receive an HTTP 200 OK response.

```
$ curl -i http://localhost:5000/_mockaco/health
HTTP/1.1 200 OK
Content-Type: text/plain
Date: Sun, 11 Jun 2023 17:46:30 GMT
Server: Kestrel
Cache-Control: no-store, no-cache
Expires: Thu, 01 Jan 1970 00:00:00 GMT
Pragma: no-cache
Transfer-Encoding: chunked

Healthy
```

## `/_mockaco/ready`

During the startup process, Mockaco asynchronously loads and caches mocks. The duration of this process may vary depending on the number of mocks being loaded. However, it's important to note that while this loading is taking place, some requests may not be served as expected, resulting in an HTTP 501 response.

To avoid this potential race condition, it is recommended to utilize the readiness endpoint.

While the mocks are still loading, the service will respond with an HTTP 503 status code. Here's an example of the response you would receive:

```
$ curl -i http://localhost:5000/_mockaco/ready
HTTP/1.1 503 Service Unavailable
Content-Type: text/plain
Date: Sun, 11 Jun 2023 17:47:55 GMT
Server: Kestrel
Cache-Control: no-store, no-cache
Expires: Thu, 01 Jan 1970 00:00:00 GMT
Pragma: no-cache
Transfer-Encoding: chunked

Unhealthy
```

Once the startup process is complete and Mockaco is ready to handle requests, the readiness endpoint will return an HTTP 200 OK status code. Here's an example:

```
$ curl -i http://localhost:5000/_mockaco/ready
HTTP/1.1 200 OK
Content-Type: text/plain
Date: Sun, 11 Jun 2023 17:50:59 GMT
Server: Kestrel
Cache-Control: no-store, no-cache
Expires: Thu, 01 Jan 1970 00:00:00 GMT
Pragma: no-cache
Transfer-Encoding: chunked

Healthy
```

### Using readiness endpoint in Dockerfile

By default, Mockaco containers does not expose health checks. However, you can create a derived Docker image and utilize the `HEALTHCHECK` instruction to ensure the container's health status is determined only after all the mocks have been loaded. Here's an example Dockerfile:

```Dockerfile
FROM natenho/mockaco
COPY Mocks /app/Mocks
HEALTHCHECK --interval=5s --timeout=3s \
    CMD curl --fail http://localhost:5000/_mockaco/ready || exit 1
```

In this Dockerfile, the `Mocks` folder is copied into the `/app/Mocks` directory within the container. You can replace Mocks with the actual path of your local Mocks folder.

The `HEALTHCHECK` instruction sets up a health check for the container. It specifies the interval and timeout for checking the health, and it runs the curl command to verify the readiness endpoint `http://localhost:5000/_mockaco/ready`. If the curl command fails (returns a non-zero exit status), the container will be considered unhealthy and exit with status code 1.

To build the derived Docker image, use the following command:

```shell
docker build -t mockaco-image .
```

Replace mockaco-image with your desired image name.

Once the image is built, you can run a container based on it, mapping the container's port 5000 to the host's port of your choice (e.g., 8080):

```shell
docker run -d -p 8080:5000 --name mockaco-container mockaco-image
```

Now the container will be running with the Mocks folder mapped to `/app/Mocks` inside it, and the health check will be performed periodically using the specified curl command.