<img src="https://image.flaticon.com/icons/svg/1574/1574279.svg" width="64px" height="64px" alt="Mockaco">

# Mockaco
A simple Roslyn-based HTTP API mock tool, leveraging AspNet Core 2.x features

# Quick Start

## Run the project
    # dotnet run
    
## Send a request and get the mocked response
    # curl -iXGET http://localhost:5000/hello/awesome
    HTTP/1.1 200 OK
    Date: Wed, 13 Mar 2019 00:22:49 GMT
    Content-Type: application/json
    Server: Kestrel
    Transfer-Encoding: chunked
    
    [
      {
        "guid": "ef82d4bd-7cd5-19d3-c63e-68ece7892b03",
        "date": "2019-03-12T00:09:45.0448302-03:00",
        "message": "awesome"
      }
    ]
    
Icon made by [Freepik](https://www.freepik.com/ "Freepik") from [www.flaticon.com](https://www.flaticon.com/ "Flaticon") is licensed by [CC 3.0 BY](http://creativecommons.org/licenses/by/3.0/ "Creative Commons BY 3.0")