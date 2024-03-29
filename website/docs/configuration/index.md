# Configuration

### App Settings

The configuration for Mockaco can be easily customized using the appsettings*.json files located within the Settings folder. These files allow you to configure various options [provided by ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration).

Here are the different appsettings*.json files and their purposes:

- *appsettings.Production.json*: Use this file to customize Mockaco when running it as an executable or dotnet tool.
- *appsettings.Docker.json*: This file is specifically used to customize Mockaco when running in a Docker container.
- *appsettings.Development.json*: When running Mockaco in debug mode, such as inside Visual Studio, you can use this file to customize its behavior. This environment is typically set through the launchSettings.json file.

These appsettings*.json files provide a convenient way to adjust Mockaco's settings based on the specific environment in which it is running.

To customize Mockaco, locate the appropriate appsettings*.json file based on your deployment scenario and modify the configuration options according to your requirements.

For instance you could override the default URLs Mockaco will listen to, just by changing the configuration like this:

```json
{
  "Urls": "http://+:8080;https://+:8443"
}
```

### Environment variables

You can also use **environment variables** to override the configuration. For example, you can set the `ASPNETCORE_URLS` environment variable to `http://+:8080;https://+:8443` to achieve the same result as the appsettings.json example.

Another way to set the configuration is through command line arguments.

### Command line

To pass mockaco specific options through **command line**, you can use the `--Mockaco` prefix. For example, to set the `DefaultHttpStatusCode` to `NotFound` you can use the following command:

```bash
dotnet mockaco run --Mockaco:DefaultHttpStatusCode=NotFound
```

Please refer to the [ASP.NET Core documentation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration) for more information on how to customize the configuration.

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

Just as Mockaco options, to pass Serilog specific options through command line, you can use the `--Serilog` prefix. For example, to set the `MinimumLevel` to `Information` you can use the following command:

```bash
dotnet mockaco run --Serilog:MinimumLevel=Information
```