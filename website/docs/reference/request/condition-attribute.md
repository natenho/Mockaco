---
sidebar_position: 3
---
# condition attribute

Any condition that evaluates to ```true``` will return the response. The condition is suitable to be used with scripting.

If omitted, empty or null, defaults to ```true```.

## Example
```json
{
  "request": {
	"condition": "<#= DateTime.Now.Second % 2 == 0 #>"
  },
  "response": {
	"status": "OK"
  }
}
```