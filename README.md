<h1>
    <img src="https://github.com/natenho/Mockaco/raw/master/src/Mockaco/Resources/mockaco-logo.svg" width="64px" height="64px" alt="Mockaco">
    Mockaco 
</h1>

Mockaco is an HTTP-based API mock server with fast setup.

[![Main Build](https://github.com/natenho/Mockaco/actions/workflows/main-release.yml/badge.svg)](https://github.com/natenho/Mockaco/actions/workflows/main-release.yml) [![Docker Pulls](https://img.shields.io/docker/pulls/natenho/mockaco)](https://hub.docker.com/repository/docker/natenho/mockaco) [![Nuget](https://img.shields.io/nuget/dt/Mockaco?color=blue&label=nuget%20downloads)](https://www.nuget.org/packages/Mockaco/) [![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fnatenho%2FMockaco.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2Fnatenho%2FMockaco?ref=badge_shield)

## Features

- Simple JSON-based configuration
- Pure C# scripting - you don't need to learn a new specific language or API to configure your mocks
- Fake data generation - built-in fake data generation
- Callback support - trigger another service call when a request hits your mocked API
- State support - stateful mock support allow a mock to be returned based on a global variable previously set by another mock
- Portable - runs in any [.NET Core compatible environment](https://github.com/dotnet/core/blob/main/release-notes/5.0/5.0-supported-os.md)

# Table of Contents

- [Get Started](#get-started)
  * [Running the application](#running-the-application)
  * [Creating a request/response template](#creating-a-requestresponse-template)
  * [Sending a request and get the mocked response](#sending-a-request-and-getting-the-mocked-response)
- [Request Template Matching](#request-template-matching)
  * [Method attribute](#method-attribute)
  * [Route attribute](#route-attribute)
  * [Condition attribute](#condition-attribute)
- [Response Template](#response-template)
  * [Delay attribute](#delay-attribute)
  * [Status attribute](#status-attribute)
  * [Headers attribute](#headers-attribute)
  * [Body attribute](#body-attribute)
  * [Indented attribute](#indented-attribute)
- [Callback Template](#callback-template)
  * [Headers attribute](#headers-attribute-1)
  * [Body attribute](#body-attribute-1)
  * [Delay attribute](#delay-attribute-1)
  * [Timeout attribute](#timeout-attribute)
  * [Indented attribute](#indented-attribute-1)
- [Scripting](#scripting)
    + [Example: Accessing request data](#example-accessing-request-data)
    + [Example: Generating fake data](#example-generating-fake-data)
- [Verification](#verification)
    + [Example: Verifying call to mocked endpoint](#example-verifying-call-to-mocked-endpoint)
      + [Verify request without body](#verify-request-without-body)
      + [Verify request with body](#verify-request-with-body)
    + [Configure custom name of verification endpoint](#configure-custom-name-of-verification-endpoint)
    + [Configure the duration of cache storing last request for verification](#configure-the-time-of-cache-storing-last-request-for-verification)
    + [Verification summary](#verification-summary)


# Get Started

## Running the application

### .NET CLI

Install and run as a dotnet tool:

```console
$ dotnet tool install -g mockaco
$ mockaco --urls "http://localhost:5000"
```

A random local port is chosen if `--urls` parameter is not provided.

### Docker

You can run Mockaco from the official [Docker image](https://hub.docker.com/r/natenho/mockaco) (replace ```/your/folder``` with an existing directory of your preference):

```console
$ docker run -it --rm -p 5000:5000 -v /your/folder:/app/Mocks natenho/mockaco
```

The port exposed by the container is 5000 (HTTP) by default.

### Sources

Or your can run it directly from sources:

```console
$ git clone https://github.com/natenho/Mockaco.git
$ cd Mockaco\src\Mockaco
$ dotnet run
```

A random local port is chosen if `--urls` parameter is not provided.

## Creating a request/response template

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

This example contains a request/response template, meaning "Whenever you receive a ```GET``` request in the route ```/ping```, respond with status ```OK``` and the body ```{ "response": "pong" }```"
	
## Sending a request and getting the mocked response

```
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

Any request with the matching HTTP method will return the response. Supported HTTP methods: GET, PUT, DELETE, POST, HEAD, TRACE, PATCH, CONNECT, OPTIONS.
If omitted, defaults to ```GET```.

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

If omitted, empty or null, defaults to base route (/).

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

Sets the HTTP status code for the response. Can be a string or a number, as defined in [System.Net.HttpStatusCode](https://docs.microsoft.com/en-us/dotnet/api/system.net.httpstatuscode?view=netcore-2.2) enumeration.

If omitted, empty or null, defaults to ```OK``` (200).

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

## Headers attribute

Sets any HTTP response headers.

### Example
```json
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

See also:

 - [Mocking XML responses](https://github.com/natenho/Mockaco/blob/master/doc/Mocking-XML.md)
 - [Mocking Binary/Raw responses](https://github.com/natenho/Mockaco/blob/master/doc/Mocking-RAW.md)

## Indented attribute

Sets the response body indentation for some structured content-types. If ommited, defaults to ```true```.

### Example
```json
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

```
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

### Example: Generating fake data

There is a ```Faker``` object available to generate fake data.

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
The faker can also generate localized data using ```Accept-Language``` HTTP header. Defaults to ```en``` (english) fake data.

# Verification

There is a default endpoint provided that let's you verify the  last call for each mocked endpoint. The default path for this endpoint is ```http://localhost:5000/_mockaco/verification?route={path to verify}```.

## Example: Verifying call to mocked endpoint

### Verify request without body

- If you have just called ```http://localhost:5000/hello/Jane Doe```, the verification endpoint
called in the following way: ```http://localhost:5000/_mockaco/verification?route=/hello/Jane Doe``` will respond like so: 
```
{
    "route": "/hello/Hello There",
    "timestamp": "14:15",
    "body": ""
}
```

### Verify request with body

- If you have just called ```http://localhost:5000/hello/Jane Doe```, with the following body:
```
{
    "username": "Jane1",
    "lastname": "Doe1"
}
```
the verification endpoint called in the following way: ```http://localhost:5000/_mockaco/verification?route=/hello/Jane Doe``` will respond like so: 
```
{
    "route": "/hello/Jane Doe",
    "timestamp": "14:39",
    "body": "{\r\n    \"username\": \"Jane1\",\r\n    \"lastname\": \"Doe1\"\r\n}"
}
```

Both JSON body and x-www-form-urlencoded body are supported.

## Configure custom name of verification endpoint

You can configure the default name of verification enpoint by modifying ```Mockaco.VerificationEndpointName``` and ```Mockaco.VerificationEndpointPrefix``` fields in ``appsettings.json`` file. So if you will rename it like so:

```
"Mockaco": {
    ...
    "VerificationEndpointName": "customVerify",
    "VerificationEndpointPrefix": "_internal"
  },
```

You will be able to access the verification endpoint on ```http://localhost:5000/_internal/customVerify```

## Configure the duration of cache storing last request for verification

Each request with the exact time of being ivoked, body and path is being stored in the internal .Net cache for 60 minutes. You can configure this time by changing

```
"Mockaco": {
    ...
    "MatchedRoutesCacheDuration": 60, 
    ...
  },
```
in ```appsettings.json```.

## Verification summary

Let's assume that you have the following endpoints mocked:
```
http://localhost:5000/hello/{message}
http://localhost:5000/test
```

With the verification functionality you can check the last performed call for each of these 2 endpoints and different variations of {message}, so if you called these 2 endpoints in the following ways:

```
curl --location --request GET 'http://localhost:5000/hello/Jane Doe'
curl --location --request GET 'http://localhost:5000/hello/Marzipan'
curl --location --request GET 'http://localhost:5000/hello/There!'
curl --location --request GET 'http://localhost:5000/test'
```

You can perform these checks:

```
http://localhost:5000/_mockaco/verification?route=/hello/Jane Doe
http://localhost:5000/_mockaco/verification?route=/hello/Marzipan
http://localhost:5000/_mockaco/verification?route=/hello/There!
http://localhost:5000/_mockaco/verification?route=/test
```
You cannot perform verification based on the generic url like ```http://localhost:5000/_mockaco/verification?route=/hello/{message}```

Icon made by [Freepik](https://www.freepik.com/ "Freepik") from [www.flaticon.com](https://www.flaticon.com/ "Flaticon") is licensed by [CC 3.0 BY](http://creativecommons.org/licenses/by/3.0/ "Creative Commons BY 3.0")


## License
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fnatenho%2FMockaco.svg?type=large)](https://app.fossa.com/projects/git%2Bgithub.com%2Fnatenho%2FMockaco?ref=badge_large)
