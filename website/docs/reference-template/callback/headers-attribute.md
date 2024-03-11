# headers attribute

Allow to set headers of the callback request.

## Example

```json
{
  "request": {
	"method": "GET"
  },
  "response": {
	"status": "OK"
  },
  "callback": {
	"method": "POST",
	"headers": {		
		"X-Foo": "Bar"
	},
	"body": {
		"message": "This request has headers!"
	}
  }
}
```