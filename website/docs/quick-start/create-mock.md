---
sidebar_position: 2
---

# Create a mock

Create a file named `ping-pong.json` under `Mocks` folder.

```json
{
  "request": {
	"method": "GET",
	"route": "ping"
  },
  "response": {
	"status": "OK",
	"body": {
	  "response": "pong"
	}
  }
}
```

This example contains a request/response template, meaning "Whenever you receive a ```GET``` request in the route ```/ping```, respond with status ```OK``` and the body ```{ "response": "pong" }```"