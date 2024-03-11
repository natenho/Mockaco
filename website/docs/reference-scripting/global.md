# Global

The Global object is a global variable that can be used to store variables that are shared between all mock requests. Its underlying storage is a `Dictionary<string, object>` object, meaning that you can store any type of variable in it. The state is not persisted between server restarts.

## Store a variable along a mock request

```
<#
    Global["my-custom-variable"] = "hello";
#>
{
  "request": {
	"method": "GET",
	"route": "ping"
  },
  "response": {
	"status": "OK",
	"body": {
	  "response": "<#=Global["my-custom-variable"] #>"
	}
  }
}
```

```shell
$ curl http://localhost:5000/ping
{
  "response": "hello"
}
```

## Store state between mock calls

The Global object is shared between all mock requests, meaning that you can store a variable in one mock and use it in another.

```
{
  "request": {
    "method": "GET",
    "route": "store"
  },
  "response": {
    "status": "OK",
    "body": {
      "response": "This request stores a variable"
    }
  }
  <#
    Global["my-custom-variable"] = "hello!";
  #>
}
```

```
{
  "request": {
    "method": "GET",
    "route": "retrieve"
  },
  "response": {
    "status": "OK",
    "body": {
      "response": "The variable is <#=Global["my-custom-variable"] #>"
    }
  }
}
```

```shell
$ curl http://localhost:5000/store
{
  "response": "This request stores a variable"
}

$ curl http://localhost:5000/retrieve
{
  "response": "The variable is hello!"
}
```

This feature can be used to simulate stateful APIs behaviors. Refer to the [Stateful mocks guide](/docs/guides/mocking-stateful) for more information.