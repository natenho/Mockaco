# Test

Send a request and get the mocked response.
Access [http://localhost:5000/ping](http://localhost:5000/ping) or call the API via curl:

```
curl -iX GET http://localhost:5000/ping

HTTP/1.1 200 OK
Date: Wed, 13 Mar 2019 00:22:49 GMT
Content-Type: application/json
Server: Kestrel
Transfer-Encoding: chunked

{
	"response": "pong"
}
```