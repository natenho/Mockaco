# Generator

This feature allows to generate mock templates based on provided specification eg. OpenApi, simply using `generate` command.

Usage:

```
mockaco generate [url/path to the specification] --provider=[provider name]
```

Example:

```
mockaco generate https://raw.githubusercontent.com/OAI/OpenAPI-Specification/main/examples/v3.0/petstore.json --provider=openapi
```

The above command will generate mocks based on Petstore OpenAPI specification example.

## Supported specifications
- [OpenApi](https://spec.openapis.org/oas/latest.html)