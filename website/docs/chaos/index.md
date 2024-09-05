# Chaos Engineering

Enabling chaos engineering, behavior different from what is expected will be randomly inserted into the calls, such as errors and delays, with this it is possible to verify how the client behaves in unforeseen situations.

## How to enable Chaos Engineering

To enable chaos it is necessary to set the 'Enabled' variable to 'true' as shown in the example below (see [Configuration](../configuration/index.md) for more details):

```
"Mockaco": {
    ...
    "Chaos": {
        "Enabled": true,
    },
    ...
  },
```

in `appsettings.json`.

## Types of answers with chaos

- Behavior: Return HTTP Error 503 (Service Unavailable)
- Exception: Return HTTP Error 500 (Internal Server Erro)
- Latency: Randomly add delay time to a call
- Result: Return HTTP Error 400 (Bad Request)
- Timeout: Waits a while and returns error 408 (Request Timeout)

## How to define parameters

Parameters are defined inside the Chaos key

```
"Mockaco": {
    ...
    "Chaos": {
        "Enabled": true,
        "ChaosRate": 20,
        "MinimumLatencyTime": 500,
        "MaximumLatencyTime": 3000,
        "TimeBeforeTimeout": 10000
    },
    ...
  },
```

in `appsettings.json`.

| Parameter          | Description                                                        | Default |
| ------------------ | ------------------------------------------------------------------ | ------- |
| Enabled            | Option to enable and disable chaos (true / false)                  | false   |
| ChaosRate          | Percentage of calls affected by chaos (0 - 100)                    | 20      |
| MinimumLatencyTime | Minimum latency in milliseconds when the latency strategy is drawn | 500     |
| MaximumLatencyTime | Maximum latency in milliseconds when the latency strategy is drawn | 3000    |
| TimeBeforeTimeout  | Time in milliseconds before timeout error                          | 10000   |
