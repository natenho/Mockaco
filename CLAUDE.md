# Mockaco Architecture Overview

Mockaco is an HTTP-based API mock server written in C# (.NET) that enables developers to quickly set up and configure mock APIs with powerful features like dynamic templating, C# scripting, state management, and chaos engineering.

## Project Structure

The Mockaco solution consists of two main projects:

### Core Projects
- **Mockaco** (Console App): Entry point and configuration setup for running the mock server
- **Mockaco.AspNetCore** (Library): Core mock server implementation with all business logic

### Test Project
- **Mockaco.AspNetCore.Tests**: Comprehensive test suite organized by feature

## High-Level Architecture

### 1. Request Processing Pipeline (Middleware Stack)

Mockaco uses a middleware-based request processing pipeline, ordered as follows:

```
ErrorHandlingMiddleware
  ↓
RequestMatchingMiddleware (matches incoming requests to mock templates)
  ↓
ResponseDelayMiddleware (simulates network delays)
  ↓
ChaosMiddleware (introduces controlled failures for chaos testing)
  ↓
ResponseMockingMiddleware (prepares and writes response)
  ↓
CallbackMiddleware (fires webhooks after response)
```

**Key Insight**: The pipeline separates concerns into distinct stages. The RequestMatchingMiddleware is the crucial decision point where incoming requests are matched against loaded mock templates. Once matched, the IMockacoContext (scoped service) carries the matched mock and transformed template through the pipeline.

### 2. Template Loading and Warm-Up System

**Flow**: File System → Template Provider → Mock Provider → Cache

#### TemplateFileProvider
- **Location**: `/Templating/Providers/TemplateFileProvider.cs`
- **Responsibility**: 
  - Discovers `.json` template files from configured directory (recursively)
  - Watches for file changes using `PhysicalFileProvider`
  - Caches templates in `IMemoryCache` with automatic invalidation on file changes
  - Supports `.mockignore` files to exclude templates from loading
  - Implements retry logic using Polly for handling file I/O locks

#### MockProvider (Warm-Up System)
- **Location**: `/MockProvider.cs`
- **Key Features**:
  - Caches transformed mocks in memory for fast lookup
  - **Warm-Up Process**: 
    1. Collects all raw templates from ITemplateProvider
    2. For each template, transforms it once with a ScriptContext (for warm-up validation)
    3. Caches successful transformations
    4. Tracks errors for templates that fail to load
    5. Sorts mocks by condition (mocks with conditions move up in priority)
  - Responds to template provider changes by re-running warm-up
  - Avoids re-transforming unchanged templates by comparing content hash (MD5)

**Critical Insight**: The warm-up system is a startup performance optimization. Templates are pre-compiled and transformed once during startup, so request handling doesn't pay the cost of C# code compilation. This is why hash-based caching is important - unchanged templates can be reused.

### 3. Request Matching System

**Architecture**: Chain of Matchers

The `RequestMatchingMiddleware` iterates through mocks and applies multiple `IRequestMatcher` implementations:

```csharp
foreach (var mock in mockProvider.GetMocks())
{
    if (await requestMatchers.AllAsync(_ => _.IsMatch(httpContext.Request, mock)))
    {
        // Request matched - attach to context and continue
    }
}
```

**Matchers** (applied in sequence, all must pass):

1. **RequestMethodMatcher**: Compares HTTP method (GET, POST, etc.). Defaults to GET if not specified.
2. **RequestRouteMatcher**: Matches URL path against template route using `RouteMatcher` (pattern matching with parameters like `/users/{id}`)
3. **RequestConditionMatcher**: Evaluates the template's "condition" field using C# scripts

**Key Insight**: The condition matcher is expensive - it transforms the template again at request time if the mock has a condition. This is why mocks with conditions are sorted to the end of the list (matched last). Conditions enable stateful routing decisions.

### 4. Template Transformation and C# Scripting System

**Architecture**: Tokenizer → Parser → Script Runner → JSON Serializer

#### TemplateTransformer
- **Location**: `/Templating/TemplateTransformer.cs`
- **Process**:
  1. Reads raw template JSON string
  2. Tokenizes using custom T4-style tokenizer (from Mono.TextTemplating)
  3. Identifies three token types:
     - **Content**: Plain JSON text
     - **Expression** (`<#= ... #>`): C# code that outputs a value
     - **Block** (`<# ... #>`): C# code that executes but produces no output
  4. For each Expression/Block token, invokes the ScriptRunner
  5. Combines outputs into final JSON string
  6. Deserializes to Template model

#### ScriptRunnerFactory
- **Location**: `/Templating/Scripting/ScriptRunnerFactory.cs`
- **Core Implementation**:
  - Uses Roslyn (`Microsoft.CodeAnalysis.CSharp.Scripting`)
  - Compiles C# code expressions into delegates at first use
  - Caches compiled runners by code string in IMemoryCache
  - Provides `ScriptContext` as globals to scripts
  - Pre-configures available imports and references:
    - System namespaces (System, System.Linq, etc.)
    - Bogus (fake data generation)
    - Newtonsoft.Json/Linq (JSON manipulation)
    - User-configured imports from MockacoOptions

**Key Insight**: This is dynamic template evaluation. Template authors write C# code inline in JSON, which gets compiled and executed. The ScriptContext is the API available to template scripts.

### 5. Script Context (Template Script API)

**Location**: `/Templating/Scripting/ScriptContext.cs`

The `ScriptContext` is the object made available to template scripts via Roslyn's globals mechanism:

```csharp
public class ScriptContext
{
    public Faker Faker { get; }                    // Fake data generation
    public ScriptContextRequest Request { get; }   // Incoming request details
    public ScriptContextResponse Response { get; } // Response (filled during request)
    public IGlobalVariableStorage Global { get; }  // Persistent state
}
```

**Request Access** (populated during request matching):
- `Request.Url`: Full URI of incoming request
- `Request.Route`: Route parameters extracted from URL pattern (e.g., `{id}`)
- `Request.Query`: Query string parameters
- `Request.Header`: HTTP headers
- `Request.Body`: Incoming body as JSON (deserialized based on content-type)

**Response Access** (populated after response preparation):
- `Response.Header`: Response headers to be sent
- `Response.Body`: Response body as JSON

**Global Storage**:
- Thread-safe `ConcurrentDictionary<string, object>`
- Allows templates to set state that persists across requests
- Writing is disabled during template warm-up (read-only)
- Writing is enabled during actual request handling (TransformAndSetVariables)

### 6. State Management and Global Variables

**Architecture**: Write-guarded, request-scoped global storage

**Flow**:
1. **During Warm-Up**: `scriptContext.Global.DisableWriting()` - Templates can read but not write
2. **During Request Processing**: `scriptContext.Global.EnableWriting()` - Templates can read and write
3. **Template Execution**:
   - `Global["varName"]` can read any variable
   - `Global["varName"] = value` can write (if enabled)

**Use Case**: Create stateful mocks where early requests set state (e.g., "user logged in") and later requests react to that state in conditions.

### 7. Request/Response Body Handling (Strategy Pattern)

#### Request Bodies
- **Location**: `/Templating/Request/`
- Strategies for different content types:
  - `JsonRequestBodyStrategy`: Parses application/json
  - `XmlRequestBodyStrategy`: Parses application/xml
  - `FormRequestBodyStrategy`: Parses application/x-www-form-urlencoded
- RequestBodyFactory selects appropriate strategy based on Content-Type header
- Bodies are normalized to JToken (JSON) for script access

#### Response Bodies
- **Location**: `/Templating/Response/`
- Strategies for generating response content:
  - `JsonResponseBodyStrategy`: Handles JToken/JObject/JArray
  - `XmlResponseBodyStrategy`: Converts JSON to XML
  - `BinaryResponseBodyStrategy`: Handles file-based responses
  - `DefaultResponseBodyStrategy`: Handles plain strings
  - `StringResponseBodyStrategy`: Fallback for other types

**Key Insight**: Both request and response handling use the Strategy pattern to support multiple content-type formats. Templates work with normalized JSON representation internally; conversion happens at serialization boundaries.

### 8. Verification System

**Location**: `/Verifyer/VerifyerExtensions.cs`

**Purpose**: Verify that a mock was called during testing

**Mechanism**:
1. When a request matches in RequestMatchingMiddleware:
   - Store request details in IMemoryCache with key: `"{RequestMatchingMiddleware} {route}"`
   - Includes: route, timestamp, headers, body
   - TTL: configurable via `MockacoOptions.MatchedRoutesCacheDuration` (default 60 min)

2. Verification endpoint (`GET /_mockaco/verification?route={route}`):
   - Queries cache for the stored request
   - Returns 200 OK with request details if found
   - Returns 404 if not found

**Limitation**: Only verifies if a route was called; doesn't deeply verify matching conditions or request body specifics by default.

### 9. Callback/Webhook System

**Location**: `/Middlewares/CallbackMiddleware.cs`

**Flow**:
1. Checked after ResponseMockingMiddleware prepares response
2. If template contains Callbacks configuration:
   - Registers fire-and-forget task on `response.OnCompleted`
   - After response is sent to client, performs callbacks
   - Callbacks are HTTP requests to external services

**Features**:
- Multiple callbacks per template
- Configurable HTTP method, URL, body, headers
- Delay support (waits before firing callback)
- Timeout configuration
- Indentation option for JSON body formatting

**Key Insight**: Callbacks are fire-and-forget; failures don't affect the mock response. Useful for webhook testing.

### 10. Chaos Engineering

**Location**: `/Chaos/` and `ChaosMiddleware.cs`

**Configuration** (`ChaosOptions`):
- `Enabled`: Toggle chaos on/off
- `ChaosRate`: Percentage of requests (1-100) that should fail

**Strategies** (pluggable via DI):
- `ChaosStrategyBehavior`: Random behavior changes
- `ChaosStrategyException`: Throws exceptions
- `ChaosStrategyLatency`: Adds random delays
- `ChaosStrategyResult`: Returns different HTTP statuses
- `ChaosStrategyTimeout`: Simulates timeouts

**Implementation**:
- Counter-based approach: Generates error list (1-100) at every 100th request
- Random strategy selection from registered implementations
- Middleware returns chaos response if request number is in error list

**Key Insight**: Used to test client resilience by introducing controlled failures without needing external chaos tools.

### 11. Dependency Injection Setup

**Location**: `/DependencyInjection/`

**Service Registration** (`MockacoServiceCollection.cs`):

```
Configuration Sources:
  └─ MockacoOptions (from config or lambda)
     ├─ ChaosOptions
     └─ TemplateFileProviderOptions

Core Services:
  ├─ IMockProvider (singleton): MockProvider
  ├─ ITemplateProvider (singleton): TemplateFileProvider
  ├─ IScriptRunnerFactory (singleton): ScriptRunnerFactory
  ├─ IFakerFactory (singleton): LocalizedFakerFactory
  │
  ├─ Request Context (scoped):
  │  ├─ IMockacoContext: MockacoContext
  │  └─ IScriptContext: ScriptContext
  │
  ├─ Request Matchers (scoped, all injected):
  │  ├─ RequestMethodMatcher
  │  ├─ RequestRouteMatcher
  │  └─ RequestConditionMatcher
  │
  ├─ Request Body Handling (transient):
  │  ├─ IRequestBodyStrategy implementations
  │  └─ IRequestBodyFactory: RequestBodyFactory
  │
  └─ Response Body Handling (transient):
     ├─ IResponseBodyStrategy implementations
     └─ IResponseBodyFactory: ResponseBodyFactory
```

**Key Insight**: Singletons are immutable, thread-safe services. Scoped services live per-request. The IMockacoContext bridges middleware stages with request-specific data.

## Configuration and Startup

### Startup Flow

1. **Program.cs**:
   - Builds IHost with Serilog logging
   - Parses command-line arguments (`--path` for templates, `--logs` for log file)
   - If CLI commands provided, executes them
   - Otherwise, runs web host

2. **Startup.cs**:
   - Calls `services.AddMockaco()` to register all services
   - Calls `app.UseMockaco()` to configure middleware pipeline

3. **MockProviderWarmUp** (Hosted Service):
   - Runs after web host starts
   - Calls `mockProvider.WarmUp()` to pre-compile templates
   - Sets `StartupHealthCheck.StartupCompleted = true`

### Configuration Sources

Templates are loaded from:
- **Default**: `Mocks/` directory relative to executable
- **Configurable via**:
  - `--path` command-line argument
  - `appsettings.json`: `"Mockaco": { "TemplateFileProvider": { "Path": "..." } }`

## Template Model Structure

```
Template
├── Request
│   ├── Method (GET, POST, etc.)
│   ├── Route (/path/{id})
│   └── Condition (C# boolean expression, optional)
├── Response
│   ├── Status (HTTP status code)
│   ├── Headers (IDictionary<string, string>)
│   ├── Body (JToken - JSON, can contain <#= scripts #>)
│   ├── Delay (milliseconds)
│   ├── File (alternative to Body)
│   └── Indented (boolean, for JSON formatting)
└── Callbacks (IEnumerable<CallbackTemplate>)
    └── CallbackTemplate
        ├── Method (HTTP method)
        ├── Url (target URL)
        ├── Body (JToken)
        ├── Headers
        ├── Delay
        └── Timeout
```

## Error Handling

**Error Tracking**:
- MockProvider.GetErrors() returns list of template loading errors
- Errors include: template name + error message
- Causes: invalid JSON output, script parser errors, other exceptions

**Error Middleware**:
- `ErrorHandlingMiddleware` catches unhandled exceptions
- Returns configured error HTTP status (default 501 Not Implemented)
- Logs errors via Serilog

## Performance Optimizations

1. **Template Caching**: Raw templates cached by hash; unchanged templates reused during warm-up
2. **Script Compilation**: Compiled runners cached by code string in IMemoryCache
3. **Memory Cache**: Scoped validation - templates transformed once at startup, not per-request
4. **Selective Re-transformation**: Only templates with conditions re-transformed at request time (checked last)
5. **Metadata Resolution**: MissingResolver caches assembly metadata resolution for Roslyn compilation

## Key Design Patterns

- **Middleware Pipeline**: Request processing separation of concerns
- **Strategy Pattern**: Multiple implementations for request/response body handling, chaos strategies
- **Factory Pattern**: ScriptRunnerFactory, RequestBodyFactory, ResponseBodyFactory
- **Dependency Injection**: Heavy use of interface-based DI via Microsoft.Extensions.DependencyInjection
- **Composite Pattern**: Multiple IRequestMatcher implementations composed via AllAsync
- **Template Method**: IRequestBodyStrategy and IResponseBodyStrategy define contract for plugins

## Extension Points

1. **Custom Script Imports**: Add namespaces via `MockacoOptions.Imports`
2. **Custom Script References**: Add assemblies via `MockacoOptions.References`
3. **Custom Request Body Strategies**: Implement `IRequestBodyStrategy`
4. **Custom Response Body Strategies**: Implement `IResponseBodyStrategy`
5. **Custom Chaos Strategies**: Implement `IChaosStrategy`

## Testing Architecture

Tests are organized by feature area:
- `Common/`: Utility and extension testing
- `Extensions/`: HTTP extension method tests
- `Middlewares/`: Middleware behavior tests
- `Templating/`: Template transformation, scripting, and request/response handling tests
