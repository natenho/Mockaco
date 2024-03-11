# Scripting

Every part of the mock file is scriptable, so you can add code to programmatically generate parts of the template.

Use C# code surrounded by `<#=` and `#>`.

:::caution Escaping

A convenient feature of Mockaco is that you don't need to escape the code inside the JSON.
Although it pays off, in the other hand, the invalid JSON file may be a little hard to indent.

:::

The mock code and generation will run for each request.

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

For multi-line code, you need to use `return` at the end of the code block:

```
<#=
  var count = 10
  var people = new int[count];

  for(var i = 0; i < count; i++ ) {
      people[i] = i + 1;
  }

  return JsonConvert.SerializeObject(people);
#>
```

## Generating fake data

There is a `Faker` object available to generate fake data.

```
{
  "request": {
	"method": "GET"
  },
  "response": {
	  "status": "OK",
	  "body": {
      "id": "<#= Faker.Random.Guid() #>",
      "number": "<#= Faker.Random.Number(1, 1000) #>",
      "fruit": "<#= Faker.PickRandom(new[] { "apple", "banana", "orange", "strawberry", "kiwi" }) #>",
      "recentDate": <#= JsonConvert.SerializeObject(Faker.Date.Recent()) #>
	  }
  }
}
```

The built-in fake data is generated via [Bogus](https://github.com/bchavez/Bogus). You can use any documented method from Bogus library.
The faker can also generate localized data using `Accept-Language` HTTP header. Defaults to `en` (english) fake data.

## Accessing request data

It's possible to access request data within the response template.
There is a `Request` object available to access request data.

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

## Accessing response data

In the same way, response data can be used within the callback request template.
There is a `Response` object available to access the generated response data.

```
{
  "request": {
    "method": "PUT",
    "route": "customers/{id}"
  },
  "response": {
    "delay": "<#=Faker.Random.Number(1,7)#>",
    "indented": true,
    "status": "201",
    "headers": {
      "X-Header-1": "1",
      "X-Header-2": "2"
    },
    "body": {
      "id": "1",
      "message": "Hello world",
	  "generatedRandomNumber": <#=Faker.Random.Number(1,10000)#>
    }
  },
  "callback": {
    "method": "POST",
    "url": "https://postman-echo.com/post",
    "timeout": 1000,
    "headers": {
      "Callback-Header-Message": "<#=Response.Body["message"]#>"
    },
    "body": {
      "responseRandomNumber": "The mocked response was <#= Response.Body["generatedRandomNumber"] #>"
    }
  }
}
```
