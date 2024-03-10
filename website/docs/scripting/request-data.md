---
sidebar_position: 1
---

# Accessing request data

There is a ```Request``` object available to access request data.

```
{
  "request": {
	"method": "PUT",
	"route": "customers/{id}"
  },
  "response": {
	"status": "OK",
	"body": {
	  "url": "<#= Request.Url #>",
	  "customerId": "<#= Request.Route["id"] #>",
	  "acceptHeader": "<#= Request.Header["Content-Type"] #>",
	  "queryString": "<#= Request.Query["dummy"] #>",
	  "requestBodyAttribute": "<#= Request.Body["address"]?[0] #>"
	}
  }
}
```

The Request object has the following properties:

- `Url`: An instance of [`Uri`](https://learn.microsoft.com/dotnet/api/system.uri) class containing request URL data.
- `Route`: A string dictionary containing route parameters.
- `Header`: A string dictionary containing request headers.
- `Query`: A string dictionary containing query parameters.
- `Body`: A [JToken](https://www.newtonsoft.com/json/help/html/t_newtonsoft_json_linq_jtoken.htm) object containing request body data.