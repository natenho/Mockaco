# file attribute

Allow to define a file to be returned in body

## Example

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

See also:

 - [Mocking binary/raw responses](/docs/guides/mocking-raw)
 - [Mocking XML responses](/docs/guides/mocking-raw)
