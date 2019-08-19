# Mocking binary/raw request/response

Mockaco is able to respond raw file contents, so it's possible to return a file, image or any static file content.

## Example 1 - Image

This example returns a static image file.

Create a file named `image.json` under `Mocks` folder. Copy any JPG image file named ```image01.jpg``` into the directory.

```json
{
  "request": {
    "method": "GET",
    "route": "/images/image01.jpg"
  },
  "response": {
    "status": "OK",
    "headers": {
      "Content-Type": "image/jpeg"
    },
    "file": "Mocks/image01.jpg"
  }
}
```

The `Content-Type` header must be set to the appropriate MIME type.
	
### Testing

The best way to test this example is to open your favorite browser and access the URL http://localhost:5000/images/image01.jpg.

## Example 2 - Binary File Download

This example instructs browsers to download any binary file.

Create a file named `binary.json` under `Mocks` folder. Copy any file named ```binary.dat``` into the directory.

```json
{
  "request": {
    "method": "GET",
    "route": "/binary.dat"
  },
  "response": {
    "status": "OK",
    "headers": {
      "Content-Type": "application/octet-stream",
      "Content-Disposition": "attachment; filename=\"binary-example.dat\""
    },
    "file": "Mocks/binary.dat"
  }
}
```

The ```Content-Disposition``` header is responsible to instruct the browser to download the file instead of guessing how to render it.

### Testing

The best way to test this example is to open your favorite browser and access the URL http://localhost:5000/binary.dat.

It will start downloading a file with the default name ```binary-example.dat``` as specified in ```filename=``` field in ```Content-Disposition``` header.

## Example 3 - Raw XML file

This example simply shows how to mock a static raw XML response so it can be sent "as-is", without any parsing.

 Create a XML file named ```mock.xml``` under `Mocks` folder.

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<root>
    <theSongName>Glycerine</theSongName>
    <theAlbum year="1994">Sixteen Stone</theAlbum>
</root>
```

Create a file named `rawxml.json` under `Mocks` folder.
```json
{
  "request": {
    "method": "GET",
    "route": "/raw-xml-example"
  },
  "response": {
    "status": "OK",
    "headers": {
      "Content-Type": "application/xml"      
    },
    "file": "Mocks/mock.xml"
  }
}
```

### Send the request and get the mocked response

```console
$ curl -iX GET http://localhost:5000/raw-xml-example
```
```http
HTTP/1.1 200 OK
Date: Tue, 13 Aug 2019 05:09:40 GMT
Content-Type: application/xml
Server: Kestrel
Transfer-Encoding: chunked

<?xml version="1.0" encoding="UTF-8" ?>
<root>
    <theSongName>Glycerine</theSongName>
    <theAlbum year="1994">Sixteen Stone</theAlbum>
</root>
```