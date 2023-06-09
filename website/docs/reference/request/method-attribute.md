---
sidebar_position: 1
---
# method attribute

Any request with the matching HTTP method will return the response. Supported HTTP methods: GET, PUT, DELETE, POST, HEAD, TRACE, PATCH, CONNECT, OPTIONS.
If omitted, defaults to ```GET```.

## Example
```json
{
  "request": {
	"method": "GET"
  },
  "response": {
	"status": "OK"
  }
}
```