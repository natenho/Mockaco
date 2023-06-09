# delay attribute

Defines a minimum response time in milliseconds. Useful to produce an artificial timeout.

If omitted, empty or null, defaults to ```0```.

## Example
```json
{
  "request": {
	"method": "GET"
  },
  "response": {
	"delay": 4000,
	"status": "OK"
  }
}
```