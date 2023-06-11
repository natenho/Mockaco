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
    "method": "GET",
    "route": "customers/{id}/accounts/{account_id}",
	  "condition": "<#= DateTime.Now.Second % 2 == 0 #>"
  },
  "response": {
    "body": "Match!"
  }
}
```

In the example above, the mock will be returned for `GET customers/1234/accounts/123ABC`, leveraging `condition` [scripting](/docs/scripting) to ensure that the mock matches only if the current time has an even second.

The condition can also be used to match based on query parameters:

```json
{
  "request": {
    "method": "GET",
    "route": "any/{myVar}",
	  "condition": "<#= Request.Query["foo"]?.ToString() == "bar" #>"
  },
  "response": {
    "body": "Hello!"
  }
}
```

The mock will be returned for `GET any/xxx?foo=bar`