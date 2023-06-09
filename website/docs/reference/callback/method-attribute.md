# method attribute

Define which [HTTP request method](https://developer.mozilla.org/docs/Web/HTTP/Methods) will be used to send the callback request.

```json
{
  "request": {
	"method": "GET"
  },
  "response": {
	"status": "OK",
	"body": {
	  "currentTime": "<#= DateTime.Now.ToString() #>"
	}
  },
  "callback": {
	"method": "DELETE",
	"url": "https://postman-echo.com/delete"
  }
}
```