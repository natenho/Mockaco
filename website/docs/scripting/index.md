# Scripting

Every part of the mock file is scriptable, so you can add code to programmatically generate parts of the template.

Use C# code surrounded by ```<#=``` and ```#>```.

:::caution Escaping

A convenient feature of Mockaco is that you don't need to escape the code inside the JSON.
Although it pays off, in the other hand, the invalid JSON file may be a little hard to indent.

:::

The mock code and generation will run for each request.

It's possible to access request data within the response template. In the same way, response data can be used within the callback request template.

The scripts are compiled and executed via [Roslyn](https://github.com/dotnet/roslyn/wiki/Scripting-API-Samples).

### Example
```json
{
  "request": {
	"method": "GET"
  },
  "response": {
	"status": "OK",
	"body": {
	  "currentYear": "<#= DateTime.Now.Year #>"
	}
  }
}
```

The code tag structure resembles [T4 Text Template Engine](https://github.com/mono/t4). In fact, this project leverages parts of T4 engine code to parse mock templates.
