# Stateful Mocks

Mockaco can persist states to allow mocking multiple behaviors for the same endpoint.
These states are simple global variables persisted in memory and available for all active mocks.

To create a global variable, simply use the `Global` object indexer and set it inside a code block, surrounded by `<#` and `#>`):

```
<#
	Global["my-custom-variable"] = "my-custom-state";
#>
```

To retrieve an existing global variable:

```
<#=Global["my-custom-variable"]#>
```

Global object can hold any object such as boolean flags, strings or your entire request payload, for instance.

Inexistent global variables will always return `null`.

## Example

Let's suppose the following scenario of 3 requests, registering a customer:

1. GET /customers returns HTTP 404 Not Found
2. POST /customers returns HTTP 201 Created
3. GET /customers returns HTTP 200 OK with the newly created customer (at this moment, the mock should reset its state so the behavior can be repeated)

## Create the request/response templates

Create `customers-get-not-found.json` under the Mocks folder:

```
{
  "request": {
	"method": "GET",
	"route": "/customers",
	"condition": "<#=Global["get-customers-state"] == null#>"
  },
  "response": {
	"status": "NotFound"	
  }
}
```

This template will match GET /customers requests whenever the global variable `get-customers-state` is `null`

Then, create `customers-post.json` under the Mocks folder:

```
{ 
  "request": {
	"method": "POST",
	"route": "/customers"
  },
  "response": {
	"status": "201"	
  }
}
<#
	// Set the state with the request payload
	Global["get-customers-state"] = Request.Body;
#>
```

This template will match POST requests to the same endpoint. It will store the entire request body into a global variable named `get-customers-state` and return HTTP 201 Created.

Last, create `customers-get-ok.json` under the Mocks folder:

```
{
  "request": {
	"method": "GET",
	"route": "/customers",
	"condition": "<#=Global["get-customers-state"] != null#>"
  },
  "response": {
	"status": "200",
	"body": <#=Global["get-customers-state"].ToString()#>
  }
}
<#
	// Reset state after the request
	Global["get-customers-state"] = null;
#>
```

This template will match GET /customers requests whenever the global variable `get-customers-state` is *not* `null`.
It will return HTTP 200 OK and output the content of the global variable named `get-customers-state` in the response body.
After that, it resets the global variable named `get-customers-state` back to `null`, allowing the cycle to be restarted.

## Testing the Example

```console
curl -iX GET http://localhost:5000/customers
```
```http
HTTP/1.1 404 Not Found
Date: Tue, 21 Jul 2020 05:56:25 GMT
Content-Type: application/json
Server: Kestrel
Content-Length: 0
```

```console
curl -iX POST \
  --url http://localhost:5000/customers \
  --header 'Content-Type: application/json' \
  --data $'{ "name": "John Doe" }'
```
```http
HTTP/1.1 201 Created
Date: Tue, 21 Jul 2020 05:58:39 GMT
Content-Type: application/json
Server: Kestrel
Content-Length: 0
```

```console
curl -iX GET http://localhost:5000/customers
```
```http
HTTP/1.1 200 OK
Date: Tue, 21 Jul 2020 06:06:05 GMT
Content-Type: application/json
Server: Kestrel
Transfer-Encoding: chunked

{
  "name": "John Doe"
}
```

```console
curl -iX GET http://localhost:5000/customers
```
```http
HTTP/1.1 404 Not Found
Date: Tue, 21 Jul 2020 05:56:25 GMT
Content-Type: application/json
Server: Kestrel
Content-Length: 0
```