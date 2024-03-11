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