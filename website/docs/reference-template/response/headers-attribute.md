# headers attribute

Sets any HTTP response headers.

## Example
```json
{
  "request": {
	"method": "GET"
  },
  "response": {    
	"status": "OK",
	"headers": {
		"X-Foo": "Bar"
	}
  }
}
```