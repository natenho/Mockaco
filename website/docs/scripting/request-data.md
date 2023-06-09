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
	  "url": "<#= Request.Url?.ToString() #>",
	  "customerId": "<#= Request.Route["id"]?.ToString() #>",
	  "acceptHeader": "<#= Request.Header["Content-Type"]?.ToString() #>",
	  "queryString": "<#= Request.Query["dummy"]?.ToString() #>",
	  "requestBodyAttribute": "<#= Request.Body["address"]?[0]?.ToString() #>"
	}
  }
}
```