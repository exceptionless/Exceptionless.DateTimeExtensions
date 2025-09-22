# Exceptionless.DateTimeExtensions

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Key Principles

All contributions must respect existing formatting and conventions specified in the `.editorconfig` file. You are a distinguished engineer and are expected to deliver high-quality code that adheres to the guidelines in the instruction files.

Let's keep pushing for clarity, usability, and excellence—both in code and user experience.

**See also:**
- [General Coding Guidelines](instructions/general.instructions.md)
- [Testing Guidelines](instructions/testing.instructions.md)

## Key Directories & Files
- `src/Exceptionless.DateTimeExtensions/` — Main library code for DateTime extensions.
- `tests/Exceptionless.DateTimeExtensions.Tests/` — Unit and integration tests for DateTime extension features.
- `build/` — Shared build props, strong naming key, and assets.
- `Exceptionless.DateTimeExtensions.slnx` — Solution file for development.

## Developer Workflows

### Build Commands
- **Main library:** `dotnet build src/Exceptionless.DateTimeExtensions/Exceptionless.DateTimeExtensions.csproj` — NEVER CANCEL: Build takes ~15 seconds. Set timeout to 60+ seconds.
- **Test project:** `dotnet build tests/Exceptionless.DateTimeExtensions.Tests/Exceptionless.DateTimeExtensions.Tests.csproj` — NEVER CANCEL: Build takes ~11 seconds. Set timeout to 60+ seconds.
- **Both projects:** Build main library first, then tests. Do NOT try to build both simultaneously with a single command.
- **Release builds:** Add `--configuration Release` to any build command.

### Test Commands
- **Run all tests:** `dotnet test tests/Exceptionless.DateTimeExtensions.Tests/Exceptionless.DateTimeExtensions.Tests.csproj` — NEVER CANCEL: Test execution takes ~5 seconds. Set timeout to 30+ seconds.
- **Release testing:** `dotnet test tests/Exceptionless.DateTimeExtensions.Tests/Exceptionless.DateTimeExtensions.Tests.csproj --configuration Release --no-build` (build first)

### VS Code Tasks (for reference only)
- **Build:** Use the VS Code task `build` or run the build commands above.
- **Test:** Use the VS Code task `test` or run the test commands above.
- **Pack:** Use the VS Code task `pack` or run `dotnet pack src/Exceptionless.DateTimeExtensions/Exceptionless.DateTimeExtensions.csproj -c Release -o artifacts`.

### Package and Format Commands
- **Create package:** `dotnet pack src/Exceptionless.DateTimeExtensions/Exceptionless.DateTimeExtensions.csproj -c Release -o artifacts` — Takes ~4 seconds.
- **Format code:** `dotnet format src/Exceptionless.DateTimeExtensions/Exceptionless.DateTimeExtensions.csproj --verify-no-changes` — Takes ~6 seconds.

## Important Constraints & Notes

### Solution File Limitations
- The repository uses `.slnx` format (Exceptionless.DateTimeExtensions.slnx) which is NOT supported by .NET SDK 8.0.119.
- DO NOT attempt to build or test using the `.slnx` file directly with current SDK versions.
- Always build individual project files using the `.csproj` paths shown above.

### Working Effectively

**Always run commands in this order:**
1. Build main library: `dotnet build src/Exceptionless.DateTimeExtensions/Exceptionless.DateTimeExtensions.csproj`
2. Build test project: `dotnet build tests/Exceptionless.DateTimeExtensions.Tests/Exceptionless.DateTimeExtensions.Tests.csproj`  
3. Run tests: `dotnet test tests/Exceptionless.DateTimeExtensions.Tests/Exceptionless.DateTimeExtensions.Tests.csproj`
4. Format verification: `dotnet format src/Exceptionless.DateTimeExtensions/Exceptionless.DateTimeExtensions.csproj --verify-no-changes`

**For release builds and packaging:**
1. Build both in Release: 
   ```
   dotnet build src/Exceptionless.DateTimeExtensions/Exceptionless.DateTimeExtensions.csproj --configuration Release
   dotnet build tests/Exceptionless.DateTimeExtensions.Tests/Exceptionless.DateTimeExtensions.Tests.csproj --configuration Release
   ```
2. Test in Release: `dotnet test tests/Exceptionless.DateTimeExtensions.Tests/Exceptionless.DateTimeExtensions.Tests.csproj --configuration Release --no-build`
3. Create package: `dotnet pack src/Exceptionless.DateTimeExtensions/Exceptionless.DateTimeExtensions.csproj --configuration Release --no-build -o artifacts`

## Validation Scenarios

**ALWAYS** manually validate any changes that affect DateTime extensions functionality by testing these core scenarios:

1. **TimeUnit parsing:** Verify `TimeUnit.Parse("1d")`, `TimeUnit.Parse("1h")`, `TimeUnit.Parse("10ms")` work correctly.
2. **DateTimeRange parsing:** Test `DateTimeRange.Parse("today", DateTime.Now)`, `DateTimeRange.Parse("yesterday", DateTime.Now)`.
3. **Elasticsearch date math:** Verify `DateTimeRange.Parse("now+1h", DateTime.Now)` and similar expressions.
4. **Bracket notation:** Test `DateTimeRange.Parse("[2023-01-01 TO 2023-12-31]", DateTime.Now)`.
5. **DateTime extensions:** Validate `.StartOfDay()` and `.EndOfDay()` methods.
6. **TimeSpan extensions:** Test `.ToWords()` method on TimeSpan objects.

**Test validation approach:**
Create a simple console application that references the built library and exercises these scenarios. All functionality should work as expected based on the examples in README.md.

## Common Tasks

### Test Results Summary
- Total tests: 550 (all pass in clean build)
- Test execution time: ~5 seconds in both Debug and Release
- Test parallelization: Disabled (single-threaded execution configured in AssemblyInfo.cs)

### Project Structure
```
src/Exceptionless.DateTimeExtensions/
├── BusinessDay.cs                    — Business day calculations
├── BusinessWeek.cs                   — Business week utilities  
├── DateMath.cs                       — Elasticsearch date math parsing
├── DateTimeExtensions.cs             — DateTime extension methods
├── DateTimeOffsetExtensions.cs       — DateTimeOffset extensions
├── DateTimeRange.cs                  — Date range parsing and operations
├── TimeSpanExtensions.cs             — TimeSpan extension methods  
├── TimeUnit.cs                       — Time unit parsing (1d, 1h, etc.)
├── TypeHelper.cs                     — Internal helper utilities
└── FormatParsers/                    — Date format parsing implementations

tests/Exceptionless.DateTimeExtensions.Tests/
├── *Tests.cs                         — Individual unit test files
├── FormatParsers/                    — Format parser specific tests
└── Properties/AssemblyInfo.cs        — Test execution configuration
```

### Dependencies & Framework
- **Main library:** .NET Standard 2.0 (for broad compatibility)
- **Test project:** .NET 8.0 with xUnit framework
- **Build dependencies:** MinVer for versioning, AsyncFixer for analysis
- **Test dependencies:** xUnit, Foundatio.Xunit, GitHubActionsTestLogger

## References & Further Reading
- [README.md](../README.md) — Full documentation with usage examples.
- [GitHub Actions workflow](https://github.com/FoundatioFx/Foundatio/.github/workflows/build-workflow.yml) — Shared build workflow details.

---

**If you are unsure about a pattern or workflow, check the README or look for similar patterns in the `src/` and `tests/` folders.**
