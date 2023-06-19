# Verification

There is a default endpoint provided that let you verify the last call for each mocked endpoint. The default path for this endpoint is ```http://localhost:5000/_mockaco/verification?route={path to verify}```.

## Example: Verifying call to mocked endpoint

### Verify request without body

If you have just called ```http://localhost:5000/hello/Jane Doe```, the verification endpoint
called in the following way: ```http://localhost:5000/_mockaco/verification?route=/hello/Jane Doe``` will respond like so: 
```
{
    "route": "/hello/Hello There",
    "timestamp": "14:15",
    "body": ""
}
```

### Verify request with body

If you have just called ```http://localhost:5000/hello/Jane Doe```, with the following body:
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

### Verify request with headers
If you have just called ```http://localhost:5000/hello/Jane Doe```, with the following headers:
```
    x-user-id:someone@email.com
    Authorization:some-bearer-token
    endtoend:b9802abd-106f-4d50-b68e-de3198777456
```
the verification endpoint called in the following way: ```http://localhost:5000/_mockaco/verification?route=/hello/Jane Doe``` will respond like so: 
```
{
    "route": "/hello/Jane Doe",
    "timestamp": "14:41",
    "headers": [
        {
            "key": "x-user-id",
            "value": "someone@email.com"
        },
        {
            "key": "Authorization",
            "value": "some-bearer-token"
        },
        {
            "key": "endtoend",
            "value": "b9802abd-106f-4d50-b68e-de3198777456"
        }
    ],
    "body": ""
}
```

### Configure hidden headers in verification endpoint
You can configure to not display headers that are not relevant to your test. By default the following headers will not be displayed:  ```Accept, Connection, Host, User-Agent, Accept-Encoding, Postman-Token, Content-Type, Content-Length.```

```
"Mockaco": {
    ...
    "HiddenHeaders": [        
        "Postman-Token",
        "Some-Irrelevant-Header",
    ],
    ...
  },
```


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

Each request with the exact time of being invoked, body and path is being stored in the internal .Net cache for 60 minutes. You can configure this time by changing

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
