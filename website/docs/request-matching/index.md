
# Request Matching

When a request is received, Mockaco follows a specific process:

Mockaco searches for files in alphabetical order.
1. It compares the request against the criteria specified in the request object of each file.
2. The first match that meets the criteria is selected. In case of ambiguity, Mockaco will prioritize mocks that have a condition.
3. If no matching Mock file is found, Mockaco returns a default response of HTTP 501 (Not Implemented). Additionally, it provides a list of possible file parsing errors.

This process ensures that Mockaco handles incoming requests and provides appropriate responses based on the available mock files. In case of any errors, the default response serves as a helpful indicator for troubleshooting.

## Criteria

The request matching is based on the [`request object`](/docs/category/request-object) defined in the mock template.

Please refer to [request object reference](/docs/category/request-object) for further details on how to use each criteria.

### Matching HTTP method

The method is the HTTP verb used in the request. It is case-insensitive. If the method is not specified, Mockaco will match `GET` method.

Example of a mock that matches a `POST` request:

```
{
  "request": {
    "method": "POST"
  },
  "response": {
    "status": "OK"
  }
}
```

```shell
$ curl -iX POST "http://localhost:5000"
HTTP/1.1 200 OK
Content-Length: 0
Content-Type: application/json
Date: Sun, 10 Mar 2024 23:10:33 GMT
Server: Kestrel
```

### Matching request route

The route follows a similar pattern to the URL, but it contains the route parameters that can be later reused inside scripts.

Example of a route with parameters that will match a request to `/customers/123`:

```
{
  "request": {
    "method": "GET",
    "route": "customers/{id}"
  },
  "response": {
    "status": "OK",
    "body": {
      "name": "John Doe"
    }
  }
}
```

```shell
$ curl -iX GET "http://localhost:5000/customers/123"
HTTP/1.1 200 OK
Content-Type: application/json
Date: Sun, 10 Mar 2024 23:05:07 GMT
Server: Kestrel
Transfer-Encoding: chunked

{
  "name": "John Doe"
}
```

The route parameters are also accessible through scripting API and can be used inside the `condition` object to match the request, or event inside the response body:

```
{
  "request": {
    "method": "GET",
    "route": "customers/{id}"
    "condition": "<#= Request.Route["id"] == "123" #>"
  },
  "response": {
    "status": "OK",
    "body": {
      "customerId": "<#= Request.Route["id"] #>"
    }
  }
}
```

```shell
$ curl -iX GET "http://localhost:5000/customers/123"
HTTP/1.1 200 OK
Content-Type: application/json
Date: Sun, 10 Mar 2024 23:06:35 GMT
Server: Kestrel
Transfer-Encoding: chunked

{
  "customerId": "123"
}
```

### Matching query parameters

Query parameters are accessible through scripting API and can be used inside the `condition` object to match the request.

This example uses a condition to check if the query parameter `my-parameter` is equal to `good`. If the condition is met, the response body will contain a message.

```
{
  "request": {
	"method": "GET",
    "condition": "<#= Request.Query["my-parameter"] == "good" #>"
  },
  "response": {
	"status": "OK",
    "headers": {
      "Content-Type": "application/json"
    },
	"body": {
	  "message": "Good query parameter"
	}
  }
}
```

```shell
$ curl -iX GET "http://localhost:5000?my-parameter=good"
HTTP/1.1 200 OK
Content-Type: application/json
Date: Sun, 10 Mar 2024 23:02:31 GMT
Server: Kestrel
Transfer-Encoding: chunked

{
  "message": "Good query parameter"
}
```

Notice that trying to get a missing query parameter will return an empty string. To check the existence of a query parameter, use the `ContainsKey` method.

```
{
  "request": {
    "method": "GET",
    "condition": "<#= Request.Query.ContainsKey("my-flag") #>"
  },
  "response": {
    "status": "OK",
    "headers": {
      "Content-Type": "application/json"
    },
    "body": {
      "message": "Query parameter exists"
    }
  }
}
```

```shell
$ curl -iX GET "http://localhost:5000?my-flag"
HTTP/1.1 200 OK
Content-Type: application/json
Date: Sun, 10 Mar 2024 23:00:37 GMT
Server: Kestrel
Transfer-Encoding: chunked

{
  "message": "Query parameter exists"
}
```

This another example uses Regex to check if the query parameter `word` contains only lowercase letters. If the condition is met, the word is returned in the response body.

```
{
  "request": {
	"method": "GET",
    "condition": "<#= Regex.IsMatch(Request.Query["word"], "^[a-z]+$") #>"
  },
  "response": {
	"status": "OK",
    "headers": {
      "Content-Type": "application/json"
    },
	"body": {
	  "word": "<#= Request.Query["word"] #>"
	}
  }
}
```

```shell
$ curl -iX GET "http://localhost:5000?word=mockaco"
HTTP/1.1 200 OK
Content-Type: application/json
Date: Sun, 10 Mar 2024 22:56:40 GMT
Server: Kestrel
Transfer-Encoding: chunked

{
  "word": "mockaco"
}
```

```shell
$ curl -iX GET "http://localhost:5000?word=Mockaco"
HTTP/1.1 501 Not Implemented
Content-Type: application/json
Date: Sun, 10 Mar 2024 22:56:23 GMT
Server: Kestrel
Transfer-Encoding: chunked

[
  {
    "Message": "Incoming request didn't match any mock"
  }
]
```