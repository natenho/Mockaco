# delay attribute

Waits for a specific amount of time in milliseconds before sending the callback.
If omitted, empty or null, defaults to 0.

This delay is useful to simulate web-hooks after processing asynchronous requests.

# Example

In this example, Mockaco will wait 5 seconds after the mocked response to send the callback.

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
	"body": {
		"message": "This request will be sent after 5 seconds"
	},
	"url": "https://postman-echo.com/post",
	"delay": 5000
  }
}
```