# Configuration

The configuration for Mockaco can be easily customized using the appsettings*.json files located within the Settings folder. These files allow you to configure various options provided by ASP.NET Core.

Here are the different appsettings*.json files and their purposes:

- *appsettings.Production.json*: Use this file to customize Mockaco when running it as an executable or dotnet tool.
- *appsettings.Docker.json*: This file is specifically used to customize Mockaco when running it within a Docker container.
- *appsettings.Development.json*: When running Mockaco in debug mode, such as inside Visual Studio, you can use this file to customize its behavior. This environment is typically set through the launchSettings.json file.

These appsettings*.json files provide a convenient way to adjust Mockaco's settings based on the specific environment in which it is running.

To customize Mockaco, locate the appropriate appsettings*.json file based on your deployment scenario and modify the configuration options according to your requirements.

For instance you could override the default URLs Mockaco will listen to, just by changing the configuration like this:

```json
{
  "Urls": "http://+:8080;https://+:8443"
}
```

Mockaco specific options are listed in the next topics.

## Mockaco

```json
{
    "Mockaco": {
        "DefaultHttpStatusCode": "OK",
        "ErrorHttpStatusCode": "NotImplemented",
        "DefaultHttpContentType": "application/json",
        "References": [],
        "Imports": [],
        "MatchedRoutesCacheDuration": 60,
        "MockacoEndpoint": "_mockaco",
        "VerificationEndpointName": "verification"
    }
}
```

### `DefaultHttpStatusCode`

Set the default HTTP status code returned when the mock does not specify one.

Default: `OK` (200)

### `ErrorHttpStatusCode`

Set the default HTTP status code in case there is no matching mock available.

Default: `NotImplemented` (501)

### `DefaultHttpContentType`

Set the default HTTP `Content-Type` header response when the mock does not specify one.

Default: `application/json`

### `References`

A list of additional references to other .NET assemblies to extend scripting engine.

Default: `[]`

### `Imports`

A list of additional namespaces to be imported and made available in scripting engine. It is the same as calling `using` in C#.

Default: `[]`

### `MatchedRoutesCacheDuration`

Set the cache duration in minutes to be used by the verification endpoint.

Default: `60`

### `MockacoEndpoint`

The exclusive endpoint to access internal features.

Default: `_mockaco`

### `VerificationEndpointName`

The name of the verification endpoint.

Default: `verification`

### `TemplateFileProvider`

Configure the mock template file provider.

#### `Path`

Define the mock template files path.

Default: `Mocks`

## `Serilog`

Configure [Serilog logger](https://github.com/serilog/serilog-settings-configuration)

