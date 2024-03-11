# timeout attribute

Specify an amount of time in milliseconds to wait for the callback request response.

Default: `5000`

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
	"timeout": 2000,
	"body": {
		"message": "This request will timeout if the server does not reply in 2 seconds"
	},
	"url": "https://postman-echo.com/post"
  }
}
```