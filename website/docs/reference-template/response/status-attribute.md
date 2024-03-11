# status attribute

Sets the HTTP status code for the response. Can be a text or numeric value, as defined in [System.Net.HttpStatusCode](https://docs.microsoft.com/en-us/dotnet/api/system.net.httpstatuscode) enumeration, so `NotFound` and `404` are both valid values for this field.

If omitted, empty or null, defaults to ```OK``` (`200`).

## Example
```json
{
  "request": {
	"method": "GET"
  },
  "response": {    
	"status": "Forbidden"
  }
}
```