# indented attribute

Sets the response body indentation for some structured content-types. If ommited, defaults to ```true```.

## Example
```json
{
  "request": {
	"method": "GET"
  },
  "response": {    
	"status": "OK",
	"body": {
	  "this": "json content",
	  "is": "supposed to be",
	  "in": "the same line"
	},
	"indented": false
  }
}
```

Result:

```
curl -iX GET http://localhost:5000

HTTP/1.1 200 OK
Date: Wed, 31 Jul 2019 02:57:30 GMT
Content-Type: application/json
Server: Kestrel
Transfer-Encoding: chunked

{"this":"json content","is":"supposed to be","in":"the same line"}
```
