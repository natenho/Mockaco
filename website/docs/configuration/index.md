# Configuration

The configuration can be made in the `appsettings.json` file inside `Settings` folder.

## `DefaultHttpStatusCode`

Set the default HTTP status code returned when the mock does not specify one.

Default: `OK` (200)

## `ErrorHttpStatusCode`

Set the default HTTP status code in case there is no matching mock available.

Default: `NotImplemented` (501)

## `DefaultHttpContentType`

Set the default HTTP `Content-Type` header response when the mock does not specify one.

Default: `application/json`

## `References`

A list of references to other .NET assemblies to extend scripting engine.

Default: `[]`

## `Imports`

A list of namespaces to be imported and made available in scripting engine.

Default: `[]`

## `MatchedRoutesCacheDuration`

Set the cache duration in minutes to be used by the verification endpoint.

Default: `60`

## `MockacoEndpoint`

The exclusive endpoint to access internal features.

Default: `_mockaco`

## `VerificationEndpointName`

The name of the verification endpoint.

Default: `verification`

## `TemplateFileProvider`

Configure the mock template file provider.

### `Path`

Define the mock template files path.

Default: `Mocks`