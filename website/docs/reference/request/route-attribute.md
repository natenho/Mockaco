---
sidebar_position: 2
---
# route attribute

Any request with the matching route will return the response. Any AspNet route template is supported.

If omitted, empty or null, defaults to base route (/).

## Example
```json
{
  "request": {
	  "route": "customers/{id}/accounts/{account_id}"
  },
  "response": {
	  "status": "OK"
  }
}
```