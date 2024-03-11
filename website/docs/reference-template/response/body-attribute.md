# body attribute

Sets the HTTP response body.

If omitted, empty or null, defaults to empty.

The ```Content-Type``` header is used to process output formatting for certain MIME types. If omitted, defaults to ```application/json```.

## Example
```json
{
  "request": {
	"method": "GET"
  },
  "response": {    
	"status": "OK",
	"body": {
	  "foo": "Bar"
	}
  }
}
```

See also:

 - [Mocking binary/raw responses](/docs/guides/mocking-raw)
 - [Mocking XML responses](/docs/guides/mocking-raw)
