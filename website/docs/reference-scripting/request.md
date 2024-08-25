# Request

The Request object has the following properties:

## Url

An instance of [`Uri`](https://learn.microsoft.com/dotnet/api/system.uri) class containing request URL data.

## Route

A [`IReadOnlyDictionary<string, string>`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2) containing route parameters. Missing keys will return an empty string. To check the existence of a key, use the `ContainsKey` method.

## Header

A [`IReadOnlyDictionary<string, string>`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2) containing request headers. Missing keys will return an empty string. To check the existence of a key, use the `ContainsKey` method.

## Query

A [`IReadOnlyDictionary<string, string>`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2) containing query parameters. Missing keys will return an empty string. To check the existence of a key, use the `ContainsKey` method.

## Body

A [`JToken`](https://www.newtonsoft.com/json/help/html/t_newtonsoft_json_linq_jtoken.htm) object containing request body data. The content of the request body is parsed as JSON and returned as a `JToken` object. The content types that Mockaco can parse are `application/json`, `application/xml`, `text/xml`, `text/plain`, `application/x-www-form-urlencoded` and `multipart/form-data`.

### Querying the body as a dictionary

The body can be queried like a dictionary of dictionaries. A simple `Request.Body["key"]?["subKey"]` will return the value of the required `subKey`. In the same way, `Request.Body[0]` would return the first item

If the key is not found, it will return `null`, so it is a good practice to use the [null conditional operator `?`](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/member-access-operators#null-conditional-operators--and-).

```
{
  "request": {
    "method": "GET",
    "route": "songs/{id}"
  },
  "response": {
    "status": "OK",
    "headers": {
      "Content-Type": "application/json"
    },
    "body": {
        "id": "<#=Request.Route["id"] #>",
        "name": "<#=Request.Body["songs"]?[0]?["name"] #>",
        "album": {
            "name": "<#=Request.Body["songs"]?[0]?["album"]?["name"] #>",
            "year": "<#=Request.Body["songs"]?[0]?["album"]?["year"] #>"
        }
    }
  }
}
```

### Querying the body using JSONPath

You can also use the [`SelectToken`](https://www.newtonsoft.com/json/help/html/M_Newtonsoft_Json_Linq_JToken_SelectToken.htm) and [`SelectTokens`](https://www.newtonsoft.com/json/help/html/M_Newtonsoft_Json_Linq_JToken_SelectTokens.htm) methods to query the body using JSONPath, which is a convenient way to traverse and filter fields from a complex object. These methods returns one or more `JToken` objects containing the selected tokens. If the token is not found, it returns `null`.

Given the following mock template:

```
{
  "request": {
    "method": "POST",
    "route": "/json_path"
  },
  "response": {
    "body": {
      "Manufacturer": "<#= Request.Body.SelectToken("Manufacturers[0].Name") #>",
      "Price": "<#= Request.Body.SelectToken("Manufacturers[0].Products[0].Price") #>",
      "ProductName": "<#= Request.Body.SelectToken("Manufacturers[1].Products[0].Name") #>",
      "AcmeManufacturer": <#= Request.Body.SelectToken("$.Manufacturers[?(@.Name == 'Acme Co')]") #>,
      "FilteredProducts": <#= JsonConvert.SerializeObject(Request.Body.SelectTokens("$..Products[?(@.Price >= 50)].Name")) #>
    }
  }
}
```

```shell
$ curl -X POST 'http://localhost:5000/json_path' \
-H 'Content-Type: application/json' \
-d '
{
  "Stores": [
    "Lambton Quay",
    "Willis Street"
  ],
  "Manufacturers": [
    {
      "Name": "Acme Co",
      "Products": [
        {
          "Name": "Anvil",
          "Price": 50
        }
      ]
    },
    {
      "Name": "Contoso",
      "Products": [
        {
          "Name": "Elbow Grease",
          "Price": 99.95
        },
        {
          "Name": "Headlight Fluid",
          "Price": 4
        }
      ]
    }
  ]
}'

{
  "Manufacturer": "Acme Co",
  "Price": "50",
  "ProductName": "Elbow Grease",
  "AcmeManufacturer": {
    "Name": "Acme Co",
    "Products": [
      {
        "Name": "Anvil",
        "Price": 50
      }
    ]
  },
  "FilteredProducts": [
    "Anvil",
    "Elbow Grease"
  ]
}
```

See also:

- [Querying JSON with SelectToken](https://www.newtonsoft.com/json/help/html/SelectToken.htms)
- [Querying JSON with LINQ](https://www.newtonsoft.com/json/help/html/QueryingLINQtoJSON.htm)
