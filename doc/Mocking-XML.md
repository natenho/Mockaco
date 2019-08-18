# Mocking XML request/response

Mockaco is able to parse XML request and make its elements available to be used in the response and/or callback templates.

## Example

This example is composed by a XML request which is transformed and returned with a different schema.

Given the XML request payload:
```xml
<?xml version="1.0" encoding="UTF-8" ?>
<root>
    <theSongName>Glycerine</theSongName>
    <theAlbum year="1994">Sixteen Stone</theAlbum>
</root>
```

## Create the request/response template
Create a file named `songs.json` under `Mocks` folder:

```
{
  "request": {
    "method": "POST",
    "route": "songs"
  },
  "response": {
    "status": "OK",
    "headers": {
      "Content-Type": "application/xml"
    },
    "body": "
<?xml version=\"1.0\" encoding=\"UTF-8\" ?>
<song>
  <name><#=Request.Body["root"]?["theSongName"]#></name>
  <album>
    <name><#=Request.Body["root"]?["theAlbum"]?["#text"]#></name>
    <year><#=Request.Body["root"]?["theAlbum"]?["@year"]#></year>
  </album>
</song>
"
  }
}
```

The `Content-Type` header must be set to `application/xml` or `text/xml`.
Notice that XML double quotes must be properly escaped, but inline C# scripts should not be escaped.

To access the request XML data and use it inside the response:
- An element **without** attributes can be directly accessed by its name:
```csharp
Request.Body["root"]?["theSongName"]
```
- An element **with** one or more attributes can be accessed using `#text` key:
```csharp
Request.Body["root"]?["theAlbum"]?["#text"]
```
- An element attribute can be accessed using `@` prefix:
```csharp
Request.Body["root"]?["theAlbum"]?["@year"]
```
	
## Send the request and get the mocked response
```console
curl -iX POST \
  --url http://localhost:5000/songs \
  --header 'Content-Type: application/xml' \
  --data $'<?xml version="1.0" encoding="UTF-8" ?>\r\n<root>\r\n	<theSongName>Glycerine</theSongName>\r\n	<theAlbum year="1994">Sixteen Stone</theAlbum>\r\n</root>'
```
```http
HTTP/1.1 200 OK
Date: Tue, 13 Aug 2019 05:09:40 GMT
Content-Type: application/xml
Server: Kestrel
Transfer-Encoding: chunked

<?xml version="1.0" encoding="UTF-8"?>
<song>
  <name>Glycerine</name>
  <album>
    <name>Sixteen Stone</name>
    <year>1994</year>
  </album>
</song>
```
