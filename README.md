<img src="https://image.flaticon.com/icons/svg/1574/1574279.svg" width="64px" height="64px" alt="Mockaco">

# Mockaco
A [Roslyn](https://github.com/dotnet/roslyn/wiki/Scripting-API-Samples)-powered HTTP-based API mock server, useful to simulate HTTP/HTTPS responses, leveraging ASP.NET Core 2+ features, built-in fake data generation and pure C# scripting.

# Quick Start

## Create a request/response template
Create a file named `PingPong.json` under `Mocks` folder:

```json
{
  "request": {
    "method": "GET",
    "route": "ping"
  },
  "response": {
    "status": "OK",
    "body": {
      "response": "pong"
    }
  }
}
```

## Run the project in a separate terminal
```# dotnet run --project src/Mockaco/Mockaco.csproj```
    
## Send a request and get the mocked response
```http
# curl -iX GET http://localhost:5000/ping
HTTP/1.1 200 OK
Date: Wed, 13 Mar 2019 00:22:49 GMT
Content-Type: application/json
Server: Kestrel
Transfer-Encoding: chunked

{
    "response": "pong"
}
```

# Request Template Matching
Use the ```request``` attribute to provide the neccessary information for the engine to decide which response will be returned. All criteria must evaluate to ```true``` to produce the response of a given template.

## Method attribute

Any request with the matching HTTP method will return the response. Supported HTTP methods: 
- GET
- PUT
- DELETE
- POST
- HEAD
- TRACE
- PATCH
- CONNECT
- OPTIONS

If ommited, defaults to ```GET```.

### Example
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

## Route attribute

Any request with the matching route will return the response. Any AspNet route template is supported.

If ommited, empty or null, defaults to base route.

### Example
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

## Condition attribute

Any condition that evaluates to ```true``` will return the response. The condition is any C# expression.

If ommited, empty or null, defaults to ```true```.

### Example
```json
{
  "request": {
    "condition": "DateTime.Now.Second % 2 == 0"
  },
  "response": {
    "status": "OK"
  }
}
```

# Response Template Scripting

## Delay attribute

Define a minimum response time in milliseconds.

If ommited, empty or null, defaults to ```0```.

### Example
```json
{
  "request": {
    "method": "GET"
  },
  "response": {
    "delay": 4000,
    "status": "OK"
  }
}
```

## Status attribute

Set the HTTP status code for the response. Can be a string or a number, as defined in [System.Net.HttpStatusCode](https://docs.microsoft.com/en-us/dotnet/api/system.net.httpstatuscode?view=netcore-2.2) enum.

If ommited, empty or null, defaults to ```OK``` (200).

### Example
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

## Body attribute

Set the HTTP response body. Supports only JSON.

If ommited, empty or null, defaults to empty.

### Example
```json
{
  "request": {
    "method": "GET"
  },
  "response": {    
    "status": "OK",
    "body": {
      "foo": "Bar"
    }
  }
}
```

# Scripting

Template attribute values are scriptable using C# code surrounded by ```${}```.

The code must be escaped to a valid JSON string.

### Example: Return current date and time
```json
{
  "request": {
    "method": "GET"
  },
  "response": {
    "status": "OK",
    "body": {
      "currentTime": "${DateTime.Now.ToString()}"
    }
  }
}
```

### Example: Access request data
```json
{
  "request": {
    "method": "PUT",
    "route": "customers/{id}"
  },
  "response": {
    "status": "OK",
    "body": {
      "url": "${Url}",
      "customerId": "${Route[\"id\"]}",
      "acceptHeader": "${Header[\"accept\"]}",
      "queryString": "${Query[\"dummy\"]}",
      "requestBodyAttribute": "${Body[\"address\"][0]}"
    }
  }
}
```

### Example: Built-in [Bogus](https://github.com/bchavez/Bogus) integration, to generate fake data
```json
{
  "request": {
    "method": "GET"
  },
  "response": {
    "status": "OK",
    "body": {
      "id": "${Faker.Random.Guid()}",
      "fruit": "${Faker.PickRandom(new[] {\"apple\",\"banana\",\"orange\",\"strawberry\",\"kiwi\"}) }",
      "recentDate": "${JsonConvert.SerializeObject(Faker.Date.Recent())}"
    }
  }
}
```

Icon made by [Freepik](https://www.freepik.com/ "Freepik") from [www.flaticon.com](https://www.flaticon.com/ "Flaticon") is licensed by [CC 3.0 BY](http://creativecommons.org/licenses/by/3.0/ "Creative Commons BY 3.0")