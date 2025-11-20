# .NET 6 → .NET 9 Migration Plan for Mockaco

## ✅ Migration Completed (2025-11-19)

### Migration Summary

The Mockaco project has been successfully migrated from .NET 6.0 to .NET 9.0! 🎉

**Status:** ✅ Fully functional and all tests passing

**Phases Completed:**
- ✅ Phase 2: Update Target Frameworks - All 3 projects migrated to .NET 9.0
- ✅ Phase 3: Update NuGet Dependencies - 18 packages updated
- ✅ Phase 4: Code Changes - 2 breaking changes fixed
- ⏸️ Phase 5: CI/CD & Infrastructure - Pending (requires GitHub Actions/Docker updates)
- ⏸️ Phase 6: Additional Validation - Core functionality validated
- ⏸️ Phase 7: Documentation Updates - Pending

### Key Changes Made

1. **Target Framework**: All projects now target `net9.0`
2. **Breaking Changes Fixed**:
   - Removed obsolete serialization constructor from `InvalidMockException`
   - Changed `Headers.Add()` to `Headers.Append()` in `ResponseMockingMiddleware`

3. **Major Package Updates**:
   - Roslyn (C# Scripting): 4.6.0 → 5.0.0
   - GitVersion: 5.12.0 → 6.5.0
   - Serilog: 7.0.0 → 9.0.0
   - xUnit: 2.4.2 → 2.9.3
   - FluentAssertions: 6.11.0 → 8.8.0
   - See full list below

4. **Test Results**: ✅ All 70 tests passing (7.0s runtime)

### Notes

- `System.CommandLine` kept at beta (2.0.0-beta1) - GA version has breaking API changes
- 6 nullable reference warnings remain in test files (pre-existing, non-critical)
- .NET 9 SDK 9.0.307 configured via `.tool-versions`

---

## Current State Analysis

**Project Structure:**
- **Mockaco.AspNetCore** (Library): .NET 6.0 library with core mock server logic
- **Mockaco** (Console): .NET 6.0 web app, packaged as dotnet tool
- **Mockaco.AspNetCore.Tests**: .NET 6.0 test project (xUnit)

**Current Issues Found:**
- Build succeeds with 1 analyzer warning (RouteHandlerAnalyzer)
- Several nullable reference warnings in test project
- Using .NET 6.0 SDK in CI/CD pipeline
- Docker images based on .NET 6.0

**Installed SDK:** .NET 6.0.428

---

## Migration Plan Overview

### Phase 1: Pre-Migration Preparation & Baseline
### Phase 2: Update Target Frameworks
### Phase 3: Update NuGet Dependencies (Conservative Approach)
### Phase 4: Code Changes
### Phase 5: Update CI/CD & Infrastructure
### Phase 6: Validation & Testing
### Phase 7: Documentation Updates

---

## Phase 1: Pre-Migration Preparation & Baseline

**Goal:** Establish a solid baseline to measure against post-migration

### 1.1 Create Migration Baseline

**Tasks:**
- [ ] Run full test suite and document current results
  ```bash
  dotnet test --configuration Release --verbosity normal test/Mockaco.AspNetCore.Tests/Mockaco.AspNetCore.Tests.csproj > baseline_tests.log 2>&1
  ```
- [ ] Build all projects in Release mode and verify outputs
  ```bash
  dotnet build --configur****ation Release > baseline_build.log 2>&1
  ```
- [ ] Document all current warnings/issues in `BASELINE_ISSUES.md`

### 1.2 Dependency Analysis

**Tasks:**
- [ ] Review .NET 7 breaking changes: https://learn.microsoft.com/en-us/dotnet/core/compatibility/7.0
- [ ] Review .NET 8 breaking changes: https://learn.microsoft.com/en-us/dotnet/core/compatibility/8.0
- [ ] Review .NET 9 breaking changes: https://learn.microsoft.com/en-us/dotnet/core/compatibility/9.0
- [ ] Review ASP.NET Core breaking changes for 7, 8, 9
- [ ] Audit all NuGet packages for .NET 9 compatibility

**Key Dependencies to Verify:**

| Package | Current Version | Notes |
|---------|----------------|-------|
| `Bogus` | 34.0.2 | Fake data generation |
| `Microsoft.CodeAnalysis.CSharp.Scripting` | 4.6.0 | Roslyn scripting - CRITICAL |
| `Newtonsoft.Json` | 13.0.3 | JSON serialization |
| `Polly` | 7.2.3 | Resilience/retry |
| `System.CommandLine` | 2.0.0-beta1 | CLI parsing (still beta) |
| `GitVersion.MsBuild` | 5.12.0 | Versioning |
| `Microsoft.Extensions.FileProviders.Physical` | 6.0.0 | File provider |
| `Serilog.AspNetCore` | 7.0.0 | Logging |
| `FluentAssertions` | 6.11.0 | Test assertions |
| `Moq` | 4.18.4 | Mocking framework |
| `Testcontainers` | 3.2.0 | Docker testing |

---

## Phase 2: Update Target Frameworks

**Goal:** Verify .NET SDK installation, update .csproj files to target .NET 9, then build to identify which dependencies require updates.

### 2.1 Verify .NET SDK Installation

Before updating target frameworks, ensure you have .NET 9 SDK installed and properly configured.

#### Check Current .NET SDK

```bash
dotnet --version
dotnet --list-sdks
```

**Expected:** You should see .NET 9.0.x SDK listed.

#### Determine Your .NET Installation Method

**Option A: System-wide Installation (Visual Studio, JetBrains Rider, or Official Installer)**

If you installed .NET via Visual Studio, JetBrains Rider, or the official Microsoft installer, your SDK is system-wide. Skip to section 2.2.

**Option B: asdf Version Manager**

If you use `asdf` to manage .NET versions:

```bash
# Check if asdf is managing dotnet
which dotnet
# If output shows path with .asdf (e.g., /Users/username/.asdf/shims/dotnet), you're using asdf

# List installed .NET versions in asdf
asdf list dotnet

# If .NET 9 is already installed, set it for this project
asdf local dotnet 9.0.100

# If .NET 9 is NOT installed, install it first:
# List available versions
asdf list all dotnet | grep "^9.0"

# Install .NET 9 SDK
asdf install dotnet 9.0.100

# Set as project-local version
asdf local dotnet 9.0.100

# Verify
dotnet --version
# Should show 9.0.100 (or your installed 9.0.x version)
```

**Note:** `asdf local` creates a `.tool-versions` file in your project root.

### 2.2 Update Target Frameworks

**Files to modify:**

1. **src/Mockaco.AspNetCore/Mockaco.AspNetCore.csproj**
   - Change `<TargetFramework>net6.0</TargetFramework>` → `<TargetFramework>net9.0</TargetFramework>`

2. **src/Mockaco/Mockaco.csproj**
   - Change `<TargetFramework>net6.0</TargetFramework>` → `<TargetFramework>net9.0</TargetFramework>`

3. **test/Mockaco.AspNetCore.Tests/Mockaco.AspNetCore.Tests.csproj**
   - Change `<TargetFramework>net6.0</TargetFramework>` → `<TargetFramework>net9.0</TargetFramework>`

### 2.3 Verify Build (Expect Errors)

After updating target frameworks, run:

```bash
dotnet restore
dotnet build
```

**Expected outcome:** Build will likely fail with errors about incompatible package versions. This is normal and expected. The errors will guide which packages need updating in Phase 3.

---

## Phase 3: Update NuGet Dependencies (Conservative Approach)

**Goal:** Update only the packages necessary to build successfully on .NET 9, following a conservative approach.

**Strategy:** Update packages in three tiers based on necessity.

### 3.1 MUST UPDATE (Required for .NET 9)

These packages are framework-specific and must match the .NET version:

| Package | Current | Target | Project | Reason |
|---------|---------|--------|---------|--------|
| `Microsoft.Extensions.FileProviders.Physical` | 6.0.0 | 9.0.0 | Mockaco | Framework-versioned package |

**Update commands:**

```bash
dotnet add src/Mockaco/Mockaco.csproj package Microsoft.Extensions.FileProviders.Physical
```

**Verify:** Run `dotnet build` after each update to check if it resolves errors.

### 3.2 RECOMMENDED UPDATE (Compatibility & Performance)

These packages should be updated for better .NET 9 compatibility, but old versions might work:

| Package | Current | Target | Project | Reason |
|---------|---------|--------|---------|--------|
| `Microsoft.CodeAnalysis.CSharp.Scripting` | 4.6.0 | 4.12.0+ | Mockaco.AspNetCore | Better C# 12 support, .NET 9 optimizations |
| `GitVersion.MsBuild` | 5.12.0 | 6.x | Mockaco.AspNetCore | Build tool compatibility |
| `Microsoft.NET.Test.Sdk` | - | 17.11.0+ | Tests | Test infrastructure |
| `coverlet.collector` | - | 6.0.2+ | Tests | Code coverage for .NET 9 |

**Update commands (only if build errors occur or you want new features):**

```bash
# Core library
dotnet add src/Mockaco.AspNetCore/Mockaco.AspNetCore.csproj package Microsoft.CodeAnalysis.CSharp.Scripting
dotnet add src/Mockaco.AspNetCore/Mockaco.AspNetCore.csproj package GitVersion.MsBuild

# Test project
dotnet add test/Mockaco.AspNetCore.Tests/Mockaco.AspNetCore.Tests.csproj package Microsoft.NET.Test.Sdk
dotnet add test/Mockaco.AspNetCore.Tests/Mockaco.AspNetCore.Tests.csproj package coverlet.collector
```

### 3.3 PROBABLY CAN KEEP (Unless Issues Arise)

These packages target .NET Standard or have broad compatibility - only update if you encounter runtime errors or want new features:

| Package | Current | Notes |
|---------|---------|-------|
| `Newtonsoft.Json` | 13.0.3 | Targets .NET Standard 2.0 - fully compatible |
| `Bogus` | 34.0.2 | Works with .NET 9, update to 35.x+ if desired |
| `Polly` | 7.2.3 | Works, but v8 has improvements (⚠️ breaking changes) |
| `Serilog.AspNetCore` | 7.0.0 | Check latest version, likely compatible |
| `FluentAssertions` | 6.11.0 | Usually backward compatible, update if desired |
| `Moq` | 4.18.4 | Update to 4.20.0+ if issues, otherwise keep |
| `Testcontainers` | 3.2.0 | Check for .NET 9 compatibility, update if needed |
| `System.CommandLine` | 2.0.0-beta1 | Still beta, keep unless GA version available |

**Update commands (only if you encounter issues or want new features):**

```bash
# Optional updates - run only if needed
dotnet add src/Mockaco.AspNetCore/Mockaco.AspNetCore.csproj package Bogus
dotnet add src/Mockaco.AspNetCore/Mockaco.AspNetCore.csproj package Serilog.AspNetCore

dotnet add test/Mockaco.AspNetCore.Tests/Mockaco.AspNetCore.Tests.csproj package FluentAssertions
dotnet add test/Mockaco.AspNetCore.Tests/Mockaco.AspNetCore.Tests.csproj package Moq
dotnet add test/Mockaco.AspNetCore.Tests/Mockaco.AspNetCore.Tests.csproj package Testcontainers

# ⚠️ CAUTION: Polly v8 has breaking changes
# dotnet add src/Mockaco.AspNetCore/Mockaco.AspNetCore.csproj package Polly
```

### 3.4 Iterative Build Process

Follow this workflow:

1. **Build after MUST updates:**
   ```bash
   dotnet build
   ```

2. **If build fails, check error messages:**
   - Package version conflicts? → Update that specific package
   - Missing package? → Add it
   - API breaking changes? → Move to Phase 4 (Code Changes)

3. **If build succeeds, run tests:**
   ```bash
   dotnet test
   ```

4. **If tests fail with dependency errors:**
   - Update specific packages from RECOMMENDED or CAN KEEP lists
   - Re-run tests

5. **Document any additional packages that needed updating**

### 3.5 Package Update Reference

**Quick reference for checking latest versions:**

```bash
# Check available versions of a package
dotnet list package --outdated

# Check specific package
nuget list <package-name>

# Or visit: https://www.nuget.org/packages/<package-name>
```

---

## Phase 4: Code Changes

### 4.1 Review and Fix Breaking Changes

**Known .NET 7-9 Breaking Areas:**

1. **Minimal API changes** (affects Startup.cs if using)
2. **JsonSerializer changes** (Newtonsoft.Json vs System.Text.Json)
3. **Nullable reference types** - stricter enforcement
4. **HttpContext changes** in ASP.NET Core
5. **Roslyn API changes** in Microsoft.CodeAnalysis

**Critical Testing Areas:**

- **Roslyn Scripting Engine** (`src/Mockaco.AspNetCore/Templating/Scripting/ScriptRunnerFactory.cs`)
  - Verify `CSharpScript.Create` and `Script.RunAsync` still work
  - Test script compilation caching
  - Verify ScriptContext globals mechanism

- **Middleware Pipeline** (`src/Mockaco.AspNetCore/Middlewares/`)
  - Test middleware execution order
  - Verify HttpContext propagation
  - Check scoped service resolution

- **Template Transformation** (`src/Mockaco.AspNetCore/Templating/TemplateTransformer.cs`)
  - Test tokenizer with complex templates
  - Verify C# expression evaluation
  - Test block statements

### 4.2 Fix Nullable Reference Warnings

**Files to review:**

1. **test/Mockaco.AspNetCore.Tests/Templating/Request/JsonRequestBodyStrategyTest.cs**
   - Line 15: Non-nullable field `_bodyStream` must contain a non-null value
   - Line 38: Dereference of a possibly null reference

2. **test/Mockaco.AspNetCore.Tests/Templating/Response/ResponseBodyFactoryTest.cs**
   - Line 14: Cannot convert null literal to non-nullable reference type

3. **test/Mockaco.AspNetCore.Tests/Middlewares/ErrorHandlingMiddlewareTest.cs**
   - Lines 50, 51, 73: Dereference of a possibly null reference

**Resolution Strategy:**
- Add null-forgiving operator (`!`) where appropriate
- Use null-conditional operators (`?.`)
- Add null checks with proper guards
- Mark fields as nullable if they can be null

### 4.3 Fix Analyzer Issues

**Current Issue:**
```
CSC : warning AD0001: Analyzer 'Microsoft.AspNetCore.Analyzers.RouteHandlers.RouteHandlerAnalyzer'
threw an exception of type 'System.ArgumentException' with message 'Syntax node is not within syntax tree'.
```

**Resolution:**
- Verify if this persists in .NET 9
- If persists, add to `.editorconfig` or suppress in project file
- Report to Microsoft if reproducible bug

---

## Phase 5: Update CI/CD & Infrastructure

### 5.1 Update GitHub Actions

**File:** `.github/workflows/main-release.yml`

**Changes needed:**

```yaml
# Line 24-27: Update .NET setup
- name: Setup .NET
  uses: actions/setup-dotnet@v4  # Updated from v1
  with:
    dotnet-version: '9.0.x'  # Updated from 6.0.x
```

**Additional Recommendations:**

Update other GitHub Actions to latest versions:
```yaml
# Line 20-23: Update checkout
- name: Checkout code
  uses: actions/checkout@v4  # Updated from v1
  with:
    fetch-depth: 0

# Line 28-31: Update GitVersion (verify compatibility)
- name: Setup GitVersion
  uses: gittools/actions/gitversion/setup@v3  # Check latest version
  with:
    versionSpec: '6.x'  # Updated from 5.x if available
```

### 5.2 Update Dockerfile

**File:** `src/Mockaco/Docker/Dockerfile`

**Changes needed:**

```dockerfile
# Line 1: Update runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
# Changed from: mcr.microsoft.com/dotnet/aspnet:6.0-alpine

# Line 6: Update SDK image
FROM mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim AS build
# Changed from: mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim
```

**Important Notes:**
- Alpine version: Verify `9.0-alpine` is available
- Debian version: Changed from `bullseye` to `bookworm` (Debian 12)
- Test multi-arch builds: `linux/amd64` and `linux/arm64`

### 5.3 Update Other CI Files (if applicable)

- [ ] Check for AppVeyor configuration
- [ ] Check for Azure Pipelines configuration
- [ ] Update any Docker Compose files
- [ ] Update Kubernetes manifests (if any)

---

## Phase 6: Validation & Testing

### 6.1 Local Validation

**Build Validation:**

```bash
# Clean everything
dotnet clean
rm -rf **/bin **/obj

# Restore dependencies
dotnet restore --verbosity normal

# Build in Debug mode
dotnet build --configuration Debug --verbosity normal

# Build in Release mode
dotnet build --configuration Release --verbosity normal

# Verify no new warnings or errors
# Compare with baseline_build.log
```

**Test Validation:**

```bash
# Run all tests
dotnet test --configuration Release --verbosity normal

# Run with detailed output
dotnet test --configuration Release --verbosity detailed --logger "console;verbosity=detailed"

# Run with code coverage
dotnet test --configuration Release --collect:"XPlat Code Coverage"

# Compare results with baseline_tests.log
```

### 6.2 Functional Testing

**Dotnet Tool Testing:**

```bash
# Build package
dotnet pack src/Mockaco/Mockaco.csproj --configuration Release

# Uninstall old version (if exists)
dotnet tool uninstall -g Mockaco

# Install new version
dotnet tool install --global --add-source ./src/Mockaco/nupkg Mockaco

# Test CLI
mockaco --help
mockaco --version

# Test running server
mockaco --path ./src/Mockaco/Mocks
```

**Core Functionality Tests:**

1. **Template Loading & Transformation**
   - [ ] Create test mock with C# scripting
   - [ ] Verify template warm-up works
   - [ ] Test script compilation and caching
   - [ ] Verify error handling for invalid templates

2. **Request Matching**
   - [ ] Test HTTP method matching (GET, POST, PUT, DELETE)
   - [ ] Test route pattern matching (`/users/{id}`)
   - [ ] Test query string matching
   - [ ] Test header matching
   - [ ] Test condition-based matching (C# expressions)

3. **Response Generation**
   - [ ] Test JSON responses
   - [ ] Test XML responses
   - [ ] Test binary/file responses
   - [ ] Test plain text responses
   - [ ] Test response headers
   - [ ] Test response status codes

4. **C# Scripting Features**
   - [ ] Test `Faker` integration (Bogus)
   - [ ] Test `Request.Query` access
   - [ ] Test `Request.Route` parameter extraction
   - [ ] Test `Request.Header` access
   - [ ] Test `Request.Body` JSON parsing
   - [ ] Test `Response.Body` manipulation
   - [ ] Test `Global` state management (read/write)

5. **Advanced Features**
   - [ ] Test response delays
   - [ ] Test callbacks/webhooks
   - [ ] Test chaos middleware (failures, latency, timeouts)
   - [ ] Test verification endpoint (`/_mockaco/verification`)

6. **Content-Type Strategies**
   - [ ] Test `application/json` requests
   - [ ] Test `application/xml` requests
   - [ ] Test `application/x-www-form-urlencoded` requests
   - [ ] Test binary file uploads

### 6.3 Docker Validation

**Build and Test:**

```bash
# Build single-platform image
docker build -f src/Mockaco/Docker/Dockerfile -t mockaco:net9-test .

# Run container
docker run -d -p 5000:5000 --name mockaco-test mockaco:net9-test

# Test endpoints
curl http://localhost:5000
curl http://localhost:5000/_mockaco/verification?route=/hello

# Check logs
docker logs mockaco-test

# Stop and remove
docker stop mockaco-test
docker rm mockaco-test

# Build multi-arch image (if needed)
docker buildx create --use --name multiarch-builder
docker buildx build --platform linux/amd64,linux/arm64 \
  -f src/Mockaco/Docker/Dockerfile \
  -t mockaco:net9-multiarch \
  --load .
```

**Docker Smoke Tests:**

- [ ] Container starts successfully
- [ ] Health check passes (if configured)
- [ ] Sample mock responds correctly
- [ ] File watching works (volume mounts)
- [ ] Certificate handling works (HTTPS)
- [ ] Environment variables applied correctly

### 6.4 Performance Testing

**Comparison Metrics (.NET 6 vs .NET 9):**

1. **Startup Performance**
   ```bash
   # Measure startup time
   time dotnet src/Mockaco/bin/Release/net9.0/Mockaco.dll
   ```
   - [ ] Startup time (cold start)
   - [ ] Template warm-up duration
   - [ ] Memory usage at startup

2. **Request Throughput**
   ```bash
   # Use Apache Bench or similar
   ab -n 10000 -c 100 http://localhost:5000/your-endpoint
   ```
   - [ ] Requests per second
   - [ ] Average response time
   - [ ] 95th percentile latency
   - [ ] 99th percentile latency

3. **Resource Usage**
   ```bash
   # Use dotnet-counters
   dotnet-counters monitor --process-id <pid>
   ```
   - [ ] CPU usage under load
   - [ ] Memory allocation rate
   - [ ] GC pause times
   - [ ] Exception count

4. **Script Compilation Performance**
   - [ ] First-time script compilation
   - [ ] Cached script execution
   - [ ] Template transformation speed

**Expectation:** .NET 9 should show improvements in most metrics, especially:
- Faster startup (ReadyToRun, tiered compilation improvements)
- Better throughput (JIT improvements)
- Lower memory (GC improvements)

### 6.5 Integration Testing

**Real-World Scenarios:**

1. **Complex Templates**
   - [ ] Test with production mock templates (if available)
   - [ ] Test deeply nested JSON structures
   - [ ] Test large response bodies (>1MB)
   - [ ] Test templates with multiple conditions

2. **State Management**
   - [ ] Test stateful workflows (login → access → logout)
   - [ ] Test concurrent requests with shared state
   - [ ] Test Global variable isolation

3. **Error Scenarios**
   - [ ] Test invalid JSON in templates
   - [ ] Test script compilation errors
   - [ ] Test runtime exceptions in scripts
   - [ ] Test missing template files
   - [ ] Test malformed requests

4. **Edge Cases**
   - [ ] Empty request bodies
   - [ ] Very long URLs
   - [ ] Special characters in route parameters
   - [ ] Large headers
   - [ ] Concurrent template file changes

---

## Phase 7: Documentation Updates

### 7.1 Update Project Documentation

**Files to update:**

- [ ] **README.md**
  - Update .NET version requirement
  - Update installation instructions
  - Update Docker instructions

- [ ] **CHANGELOG.md** (or create)
  - Document migration to .NET 9
  - Note any breaking changes
  - List dependency updates

- [ ] **Docker Documentation**
  - Update base image references
  - Update Docker Hub tags

- [ ] **CI/CD Documentation**
  - Update pipeline descriptions
  - Note new GitHub Actions versions

### 7.2 Create Migration Guide

Create `MIGRATION_GUIDE.md` for users:

- How to upgrade from .NET 6 to .NET 9
- Breaking changes (if any)
- Required actions for users
- FAQ section

### 7.3 Update Package Metadata

**NuGet Package Description:**

Update package descriptions in `.csproj` files to mention .NET 9:

```xml
<Description>
  ASP.NET Core pipeline to mock HTTP requests/responses (.NET 9),
  useful to stub services and simulate dynamic API responses,
  leveraging ASP.NET Core features, built-in fake data generation
  and pure C# scripting
</Description>
```

---

## Breaking Change Checklist

### Critical Areas to Test

**ASP.NET Core Pipeline:**
- [ ] Middleware execution order
- [ ] HttpContext behavior
- [ ] Request/Response body handling
- [ ] File provider changes
- [ ] Dependency injection container
- [ ] Scoped service lifetime
- [ ] IMemoryCache behavior

**Roslyn Scripting:**
- [ ] Script compilation still works
- [ ] ScriptContext globals accessibility
- [ ] Assembly references resolution
- [ ] Namespace imports
- [ ] Script caching mechanism
- [ ] Error handling and reporting

**JSON Handling:**
- [ ] Newtonsoft.Json compatibility
- [ ] JToken/JObject/JArray behavior
- [ ] Serialization/deserialization
- [ ] JsonConverter usage

**Testing Framework:**
- [ ] xUnit compatibility
- [ ] Moq behavior changes
- [ ] FluentAssertions syntax
- [ ] Testcontainers compatibility
- [ ] Test discovery and execution

**CLI & Packaging:**
- [ ] System.CommandLine (still beta)
- [ ] Dotnet tool packaging
- [ ] Tool installation and execution
- [ ] Command-line argument parsing

---

## Risk Assessment

### High Risk Areas

**1. Roslyn Scripting (Microsoft.CodeAnalysis)**
- **Risk Level:** HIGH
- **Impact:** Core functionality - template transformation
- **Mitigation:**
  - Test with comprehensive script samples
  - Review Roslyn 4.6 → 4.12+ changelog
  - Create regression tests for all script features
  - Test script compilation caching

**2. ASP.NET Core Middleware**
- **Risk Level:** MEDIUM-HIGH
- **Impact:** Request processing pipeline
- **Mitigation:**
  - Test all middleware in sequence
  - Verify HttpContext state propagation
  - Check scoped service resolution
  - Test error handling middleware

**3. Docker Multi-arch Build**
- **Risk Level:** MEDIUM
- **Impact:** Container deployment
- **Mitigation:**
  - Test both linux/amd64 and linux/arm64
  - Verify Alpine Linux compatibility
  - Test Debian bookworm compatibility
  - Check certificate handling

**4. NuGet Package Updates**
- **Risk Level:** MEDIUM
- **Impact:** Dependency compatibility
- **Mitigation:**
  - Update one package at a time
  - Run tests after each update
  - Check for breaking changes in each package
  - Pin versions in .csproj

### Medium Risk Areas

**5. System.CommandLine (Beta Package)**
- **Risk Level:** MEDIUM
- **Impact:** CLI argument parsing
- **Mitigation:**
  - Check if GA version available
  - Test all CLI commands thoroughly
  - Have rollback plan if breaking changes

**6. Polly (v7 → v8 upgrade)**
- **Risk Level:** MEDIUM
- **Impact:** Retry logic in TemplateFileProvider
- **Mitigation:**
  - Review Polly v8 migration guide
  - May need code changes
  - Consider staying on v7 if v8 has breaking changes

### Low Risk Areas

- Bogus (stable API)
- Serilog (stable logging)
- xUnit (stable test framework)
- FluentAssertions (stable)
- Newtonsoft.Json (mature, stable)

---

## Rollback Plan

If critical issues are found during migration:

### Immediate Rollback

```bash
# Revert all changes
git reset --hard origin/master

# Or revert specific commits
git revert <commit-hash>

# Delete migration branch
git branch -D feature/migrate-to-net9
```

### Partial Rollback Options

**Option 1: Target .NET 8 LTS Instead**
- Change target framework to `net8.0`
- Update packages to .NET 8 versions
- Lower risk, longer support (until Nov 2026)

**Option 2: Multi-Targeting**
- Support both .NET 6 and .NET 9
- Update `.csproj`:
  ```xml
  <TargetFrameworks>net6.0;net9.0</TargetFrameworks>
  ```
- More complex but provides compatibility

**Option 3: Incremental Migration**
- First migrate to .NET 7 (EOL May 2024 - skip this)
- Then migrate to .NET 8 LTS (recommended)
- Finally migrate to .NET 9

### Issue Documentation

If blocking issues found:
1. Document in GitHub Issues
2. Create reproduction steps
3. Check if .NET 9 bug or project-specific
4. Report to Microsoft if .NET bug
5. Maintain .NET 6 branch until resolved

---

## Post-Migration Tasks

### Release Preparation

- [ ] Update version number (GitVersion)
- [ ] Create detailed release notes
- [ ] Document breaking changes (if any)
- [ ] Update CHANGELOG.md

### Package Publishing

- [ ] Publish NuGet packages
  - Mockaco (dotnet tool)
  - Mockaco.AspNetCore (library)
- [ ] Publish Docker images
  - `natenho/mockaco:latest`
  - `natenho/mockaco:<version>`
  - Multi-arch: `linux/amd64`, `linux/arm64`

### Monitoring & Support

- [ ] Monitor GitHub Issues for user problems
- [ ] Monitor NuGet download metrics
- [ ] Monitor Docker Hub pull metrics
- [ ] Watch for .NET 9 patch releases
- [ ] Update dependencies regularly

### Documentation

- [ ] Update website documentation (if applicable)
- [ ] Update README badges (if showing .NET version)
- [ ] Update installation guides
- [ ] Create blog post/announcement (optional)

---

## Recommended Approach

### Option A: Direct Migration .NET 6 → .NET 9
**Pros:**
- Faster, single migration
- Latest features immediately
- Less work overall

**Cons:**
- Higher risk
- Harder to debug issues
- Skips LTS version (.NET 8)

### Option B: Incremental Migration .NET 6 → .NET 8 → .NET 9 ⭐ RECOMMENDED
**Pros:**
- Lower risk per step
- Easier to isolate breaking changes
- .NET 8 is LTS (supported until Nov 2026)
- Safe fallback option

**Cons:**
- More iterations
- Takes longer
- More testing cycles

### Recommendation

Given that .NET 9 is STS (Standard Term Support until May 2026), I recommend:

1. **First Phase:** Migrate .NET 6 → .NET 8 (LTS)
   - Follow this plan, but target .NET 8
   - Validate thoroughly
   - Release as stable version
   - .NET 8 support until November 2026

2. **Second Phase:** Evaluate .NET 9 migration
   - After .NET 8 is stable in production
   - Monitor .NET 9 adoption and stability
   - Migrate when confident
   - .NET 9 support until May 2026

**Why this approach:**
- Safety: LTS provides longer support window
- Stability: .NET 8 is more mature
- Flexibility: Easy to stay on .NET 8 if .NET 9 has issues
- Best practices: Many enterprises prefer LTS releases

---

## Timeline Estimate

### Fast Track (Direct to .NET 9)
- **Phase 1 (Preparation):** 1-2 days
- **Phase 2 (Target Frameworks):** 0.25 day
- **Phase 3 (Dependencies):** 0.5-1 day
- **Phase 4 (Code Changes):** 1-2 days
- **Phase 5 (CI/CD Updates):** 0.5 day
- **Phase 6 (Validation):** 2-3 days
- **Phase 7 (Documentation):** 1 day

**Total:** 6-10 days

### Recommended Track (via .NET 8)
- **Phase 1-7 for .NET 8:** 6-10 days
- **Stabilization period:** 1-2 weeks
- **Phase 1-7 for .NET 9:** 4-6 days (faster, leverages .NET 8 work)

**Total:** 3-4 weeks

---

## Success Criteria

Migration is successful when:

- [ ] All projects build without errors
- [ ] All tests pass (same or better coverage)
- [ ] No new warnings introduced
- [ ] Dotnet tool installs and runs correctly
- [ ] Docker image builds and runs correctly
- [ ] Performance metrics equal or better than .NET 6
- [ ] All functional tests pass
- [ ] No regressions in existing features
- [ ] CI/CD pipeline passes
- [ ] Documentation updated
- [ ] NuGet packages published successfully
- [ ] Docker images published successfully

---

## Contact & Support

For issues during migration:
- GitHub Issues: https://github.com/natenho/Mockaco/issues
- .NET 9 Migration Guide: https://learn.microsoft.com/en-us/dotnet/core/migration/
- ASP.NET Core Migration: https://learn.microsoft.com/en-us/aspnet/core/migration/

---

## Appendix A: Useful Commands

```bash
# Check .NET version
dotnet --version
dotnet --list-sdks
dotnet --list-runtimes

# Clean build
dotnet clean
dotnet restore
dotnet build

# Run tests with different options
dotnet test --verbosity detailed
dotnet test --collect:"XPlat Code Coverage"
dotnet test --filter "FullyQualifiedName~RequestMatching"

# Pack and publish
dotnet pack --configuration Release
dotnet nuget push **/*.nupkg --source https://api.nuget.org/v3/index.json

# Docker commands
docker build -t mockaco:test .
docker run -p 5000:5000 mockaco:test
docker logs <container-id>
docker exec -it <container-id> /bin/sh

# Performance monitoring
dotnet-counters monitor --process-id <pid>
dotnet-trace collect --process-id <pid>
```

## Appendix B: Breaking Changes References

- .NET 7: https://learn.microsoft.com/en-us/dotnet/core/compatibility/7.0
- .NET 8: https://learn.microsoft.com/en-us/dotnet/core/compatibility/8.0
- .NET 9: https://learn.microsoft.com/en-us/dotnet/core/compatibility/9.0
- ASP.NET Core 7: https://learn.microsoft.com/en-us/aspnet/core/migration/60-70
- ASP.NET Core 8: https://learn.microsoft.com/en-us/aspnet/core/migration/70-80
- ASP.NET Core 9: https://learn.microsoft.com/en-us/aspnet/core/migration/80-90

---

**Last Updated:** 2025-11-19
**Plan Version:** 1.0
**Target Framework:** .NET 9.0
