# body attribute

Sets the content to be sent to another server.

If omitted, empty or null, defaults to empty.

## Example

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
	"method": "POST",
	"body": {
		"message": "The response was <#= Response.Body["currentTime"]?.ToString() #>"
	},
	"url": "https://postman-echo.com/post",
  }
}
```

:::tip Scripting tip

The request data and the generated response data can be accessed in callback to produce dynamic callback content.

:::