# Response

The Response object is accessible **after** the mock is generated. This object contains response that was sent to the client. It has the following properties:

## Header

A [`IReadOnlyDictionary<string, string>`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2) containing response headers. Missing keys will return an empty string. To check the existence of a key, use the `ContainsKey` method.

## Body

A [`JToken`](https://www.newtonsoft.com/json/help/html/t_newtonsoft_json_linq_jtoken.htm) object containing response body data.