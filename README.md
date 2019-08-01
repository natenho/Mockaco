<img src="https://image.flaticon.com/icons/svg/1574/1574279.svg" width="64px" height="64px" alt="Mockaco">

# Mockaco [![Build status](https://ci.appveyor.com/api/projects/status/0e0qfnp2kobgakl6/branch/master?svg=true)](https://ci.appveyor.com/project/natenho/mockaco/branch/master)
Mockaco is an HTTP-based API mock server with fast setup, featuring:

- Simple JSON-based configuration
- Pure C# scripting - you don't need to learn a new specific language or API to configure your mocks
- Fake data generation - built-in hassle-free fake data generation
- Callback support - trigger another service call when a request hits your mocked API

# Quick Start

## Running the application

You can run Mockaco from the official [Docker image](https://hub.docker.com/r/natenho/mockaco):

```
docker run -it --rm -p 5000:80 -v C:\Mocks:/app/Mocks natenho/mockaco
```

Or you can run the [latest binaries](https://github.com/natenho/Mockaco/releases/latest/download/Mockaco.Web.Site.zip):

```
dotnet Mockaco.dll
```

Or your can run it directly from sources using [.NET Core](https://dotnet.microsoft.com/download):

```
git clone https://github.com/natenho/Mockaco.git
cd Mockaco\src\Mockaco
dotnet run
```

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
	
## Send a request and get the mocked response
```http
curl -iX GET http://localhost:5000/ping

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
Use the ```request``` attribute to provide the necessary information for the engine to decide which response will be returned. All criteria must evaluate to ```true``` to produce the response of a given template.

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

If omitted, defaults to ```GET```.

### Example
```
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

If omitted, empty or null, defaults to base route (/).

### Example
```
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

Any condition that evaluates to ```true``` will return the response. The condition is suitable to be used with C# scripting.

If omitted, empty or null, defaults to ```true```.

### Example
```
{
  "request": {
	"condition": "<#= DateTime.Now.Second % 2 == 0 #>"
  },
  "response": {
	"status": "OK"
  }
}
```

# Response Template

## Delay attribute

Defines a minimum response time in milliseconds.

If omitted, empty or null, defaults to ```0```.

### Example
```
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

Sets the HTTP status code for the response. Can be a string or a number, as defined in [System.Net.HttpStatusCode](https://docs.microsoft.com/en-us/dotnet/api/system.net.httpstatuscode?view=netcore-2.2) enumeration.

If omitted, empty or null, defaults to ```OK``` (200).

### Example
```
{
  "request": {
	"method": "GET"
  },
  "response": {    
	"status": "Forbidden"
  }
}
```

## Headers attribute

Sets any HTTP response headers.

### Example
```
{
  "request": {
	"method": "GET"
  },
  "response": {    
	"status": "OK",
	"headers": {
		"X-Foo": "Bar"
	}
  }
}
```

## Body attribute

Sets the HTTP response body.

If omitted, empty or null, defaults to empty.

The ```Content-Type``` header is used to process output formatting for certain MIME types. If omitted, defaults to ```application/json```.

### Example
```
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

## Indented attribute

Sets the response body indentation for some structured content-types. If ommited, defaults to ```true```.

### Example
```
{
  "request": {
	"method": "GET"
  },
  "response": {    
	"status": "OK",
	"body": {
	  "this": "json content",
	  "is": "supposed to be",
	  "in": "the same line"
	},
	"indented": false
  }
}
```

Result:

```http
curl -iX GET http://localhost:5000

HTTP/1.1 200 OK
Date: Wed, 31 Jul 2019 02:57:30 GMT
Content-Type: application/json
Server: Kestrel
Transfer-Encoding: chunked

{"this":"json content","is":"supposed to be","in":"the same line"}
```

# Callback Template

Calls another API whenever a request arrives.

```
{
  "request": {
	"method": "GET"
  },
  "response": {
	"status": "OK",
	"body": {
	  "currentTime": "<#= DateTime.Now.ToString() #>"
	}
  },
  "callback": {
	"method": "POST",
	"timeout": 2000,
	"headers": {		
		"X-Foo": "Bar"
	},
	"body": {
		"message": "The response was <#= Response.Body["currentTime"]?.ToString() #>"
	},
	"url": "https://postman-echo.com/post",
	"delay": 5000
  }
}
```

## Headers attribute

Sets any HTTP callback request headers.

## Body attribute

Sets the HTTP callback request body.

If omitted, empty or null, defaults to empty.

The ```Content-Type``` header is used to process output formatting for certain MIME types. If omitted, defaults to ```application/json```.

## Delay attribute

The time to wait after a response before performing the callback.

## Timeout attribute

Defines a time in milliseconds to wait the callback response before cancelling the operation.

## Indented attribute

Sets the callback body indentation for some structured content-types. If ommited, defaults to ```true```.

# Scripting

Every part of the template file is scriptable, so you can add code to programatically generate parts of the template.

Use C# code surrounded by ```<#=``` and ```#>```.

The template code and generation will run for each request.

It's possible to access request data within the response template. In the same way, response data can be used within the callback request template.

The scripts are compiled and executed via [Roslyn](https://github.com/dotnet/roslyn/wiki/Scripting-API-Samples).

### Example
```
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

### Example: Accessing request data
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

### Example: Generating fake data
```
{
  "request": {
	"method": "GET"
  },
  "response": {
	"status": "OK",
	"body": {
	  "id": "<#= Faker.Random.Guid() #>",
	  "fruit": "<#= Faker.PickRandom(new[] { "apple", "banana", "orange", "strawberry", "kiwi" }) #>",
	  "recentDate": <#= JsonConvert.SerializeObject(Faker.Date.Recent()) #>
	}
  }
}
```

The built-in fake data is generated via [Bogus](https://github.com/bchavez/Bogus).

Icon made by [Freepik](https://www.freepik.com/ "Freepik") from [www.flaticon.com](https://www.flaticon.com/ "Flaticon") is licensed by [CC 3.0 BY](http://creativecommons.org/licenses/by/3.0/ "Creative Commons BY 3.0")
