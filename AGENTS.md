# Agent Guidelines for Exceptionless.DateTimeExtensions

You are an expert .NET engineer working on Exceptionless.DateTimeExtensions, a focused utility library providing DateTimeRange, Business Day/Week calculations, Elasticsearch-compatible date math parsing, and extension methods for DateTime, DateTimeOffset, and TimeSpan. Your changes must maintain backward compatibility, correctness across edge cases (especially timezone handling), and parsing reliability. Approach each task methodically: research existing patterns, make surgical changes, and validate thoroughly.

**Craftsmanship Mindset**: Every line of code should be intentional, readable, and maintainable. Write code you'd be proud to have reviewed by senior engineers. Prefer simplicity over cleverness. When in doubt, favor explicitness and clarity.

## Repository Overview

Exceptionless.DateTimeExtensions provides date/time utilities for .NET applications:

- **DateTimeRange** - Parse and manipulate date ranges with natural language, bracket notation, and Elasticsearch date math
- **DateMath** - Standalone Elasticsearch-style date math expression parser with timezone support
- **TimeUnit** - Parse time unit strings (`1d`, `5m`, `100ms`, `1nanos`) into TimeSpan values
- **BusinessDay / BusinessWeek** - Calculate business hours, next business day, and business time spans
- **DateTime Extensions** - Age strings, epoch conversions, start/end of periods, safe arithmetic, navigation helpers
- **DateTimeOffset Extensions** - Mirror of DateTime extensions for DateTimeOffset with offset-aware operations
- **TimeSpan Extensions** - Human-readable formatting (`ToWords`), year/month extraction, rounding, `AgeSpan` struct

Design principles: **parse flexibility**, **timezone correctness**, **comprehensive edge case handling**, **netstandard2.0 compatibility**.

## Quick Start

```bash
# Build
dotnet build Exceptionless.DateTimeExtensions.slnx

# Test
dotnet test Exceptionless.DateTimeExtensions.slnx

# Format code
dotnet format Exceptionless.DateTimeExtensions.slnx
```

## Project Structure

```text
src
└── Exceptionless.DateTimeExtensions
    ├── BusinessDay.cs                     # Single business day (day of week, start/end time)
    ├── BusinessWeek.cs                    # Business week with time calculations
    ├── DateMath.cs                        # Elasticsearch date math parser (Parse/TryParse)
    ├── DateTimeExtensions.cs              # DateTime extension methods
    ├── DateTimeOffsetExtensions.cs        # DateTimeOffset extension methods
    ├── DateTimeRange.cs                   # Date range with parsing via format parser chain
    ├── TimeSpanExtensions.cs              # TimeSpan extensions and AgeSpan struct
    ├── TimeUnit.cs                        # Time unit string parser (e.g., "1d", "5ms")
    ├── TypeHelper.cs                      # Reflection utility for parser discovery
    └── FormatParsers
        └── FormatParsers
            ├── IFormatParser.cs           # Full-string parser interface
            ├── PriorityAttribute.cs       # Parser ordering attribute
            ├── Helper.cs                  # Shared constants (month names, time names)
            ├── TwoPartFormatParser.cs     # Range parser: [start TO end], start - end
            ├── ExplicitDateFormatParser.cs
            ├── MonthDayFormatParser.cs
            ├── MonthFormatParser.cs
            ├── MonthRelationFormatParser.cs
            ├── NamedDayFormatParser.cs
            ├── RelationAmountTimeFormatParser.cs
            ├── SingleTimeRelationFormatParser.cs
            ├── YearFormatParser.cs
            └── PartParsers
                ├── IPartParser.cs         # Part parser interface (regex + parse)
                ├── DateMathPartParser.cs   # Delegates to DateMath.TryParseFromMatch
                ├── WildcardPartParser.cs   # Handles * for open-ended ranges
                ├── NamedDayPartParser.cs
                ├── AmountTimeRelationPartParser.cs
                ├── ExplicitDatePartParser.cs
                ├── MonthDayPartParser.cs
                ├── MonthPartParser.cs
                ├── MonthRelationPartParser.cs
                ├── SingleTimeRelationPartParser.cs
                └── YearPartParser.cs
tests
└── Exceptionless.DateTimeExtensions.Tests
    ├── DateMathTests.cs                   # Comprehensive date math parsing tests
    ├── DateTimeRangeTests.cs              # Range parsing and manipulation tests
    ├── DateTimeExtensionsTests.cs         # DateTime extension method tests
    ├── TimeSpanExtensionTests.cs          # TimeSpan extension tests
    ├── TimeUnitTests.cs                   # Time unit parsing tests
    ├── BusinessDayTests.cs                # Business day/week calculation tests
    ├── RandomHelper.cs                    # Test utility for random date generation
    └── FormatParsers
        ├── FormatParserTestsBase.cs       # Base class for format parser tests
        ├── TwoPartFormatParserTests.cs
        ├── (other format parser tests)
        └── PartParsers
            ├── PartParserTestsBase.cs     # Base class for part parser tests
            └── (other part parser tests)
```

### Architecture: Parser Chain

The library uses a **priority-ordered parser chain** for `DateTimeRange.Parse()`:

1. **`IFormatParser` implementations** (10 parsers, auto-discovered via reflection) try to match the entire input string in priority order
2. **`TwoPartFormatParser`** handles two-sided ranges (`[start TO end]`, `start - end`) and delegates each side to **`IPartParser` implementations** (10 parsers, also priority-ordered)
3. **`DateMath`** is usable standalone or through `DateMathPartParser` within ranges
4. Parsers are discovered via `TypeHelper.GetDerivedTypes<T>()` and sorted by `[Priority]` attribute

## Coding Standards

### Style & Formatting

- Follow `.editorconfig` rules (file-scoped namespaces enforced, IDE0005 as error)
- Follow [Microsoft C# conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Run `dotnet format` to auto-format code
- Match existing file style; minimize diffs
- No code comments unless necessary—code should be self-explanatory

### Code Quality

- Write complete, runnable code—no placeholders, TODOs, or `// existing code...` comments
- Use modern C# features compatible with **netstandard2.0**: be mindful of API availability
- Follow SOLID, DRY principles; remove unused code and parameters
- Clear, descriptive naming; prefer explicit over clever
- Use `ConfigureAwait(false)` in library code (not in tests)

### Parsing & Date Math Patterns

- **Regex-based parsing**: Part parsers expose a `Regex` property; format parsers use regex internally
- **Priority ordering**: Lower `[Priority]` values run first—put more specific/common parsers earlier
- **Timezone preservation**: When parsing dates with explicit timezones (`Z`, `+05:00`), always preserve the original offset
- **Upper/lower limit rounding**: `/d` rounds to start of day for lower limits, end of day for upper limits
- **Null return for no match**: Parsers return `null` when they can't handle the input; the chain tries the next parser
- **Date math expressions**: Follow [Elasticsearch date math syntax](https://www.elastic.co/guide/en/elasticsearch/reference/current/common-options.html#date-math) — anchors (`now`, explicit dates with `||`), operations (`+`, `-`, `/`), units (`y`, `M`, `w`, `d`, `h`, `H`, `m`, `s`)

### Common Patterns

- **Extension methods**: Group by target type (`DateTimeExtensions.cs`, `DateTimeOffsetExtensions.cs`, `TimeSpanExtensions.cs`)
- **Safe arithmetic**: Use overflow-protected add/subtract methods (`SafeAdd`, `SafeSubtract`) to avoid `ArgumentOutOfRangeException`
- **Start/End helpers**: Provide `StartOf*` and `EndOf*` for all time periods (minute, hour, day, week, month, year)
- **Exceptions**: Use `ArgumentException` for invalid input. Use `FormatException` for parsing failures. Return `false` from `TryParse` methods instead of throwing.

### Single Responsibility

- Each class has one reason to change
- Methods do one thing well; extract when doing multiple things
- Keep files focused: one primary type per file
- Each parser handles one specific format pattern
- If a method needs a comment explaining what it does, it should probably be extracted

### Performance Considerations

- **Avoid allocations in hot paths**: Parsing methods are called frequently; minimize string allocations
- **Compiled regex**: Use `RegexOptions.Compiled` for frequently-used patterns
- **Span-based parsing**: Where compatible with netstandard2.0, prefer span-based approaches
- **Profile before optimizing**: Don't guess—measure
- **Cache parser instances**: Parsers are discovered once via reflection and reused

## Making Changes

### Before Starting

1. **Gather context**: Read related files, search for similar implementations, understand the full scope
2. **Research patterns**: Find existing usages of the code you're modifying using grep/semantic search
3. **Understand completely**: Know the problem, side effects, and edge cases before coding
4. **Plan the approach**: Choose the simplest solution that satisfies all requirements
5. **Check dependencies**: Verify you understand how changes affect dependent code

### Pre-Implementation Analysis

Before writing any implementation code, think critically:

1. **What could go wrong?** Consider timezone edge cases, overflow/underflow, leap years, DST transitions
2. **What are the parsing edge cases?** Ambiguous input, whitespace, case sensitivity, partial matches
3. **What assumptions am I making?** Validate each assumption against existing tests
4. **Is this the root cause?** Don't fix symptoms—trace to the core problem
5. **Is there existing code that does this?** Search before creating new utilities

### Test-First Development

**Always write or extend tests before implementing changes:**

1. **Find existing tests first**: Search for tests covering the code you're modifying
2. **Extend existing tests**: Add test cases to existing test classes/methods when possible for maintainability
3. **Write failing tests**: Create tests that demonstrate the bug or missing feature
4. **Implement the fix**: Write minimal code to make tests pass
5. **Refactor**: Clean up while keeping tests green
6. **Verify edge cases**: Add tests for boundary conditions, timezone handling, and error paths

**Why extend existing tests?** Consolidates related test logic, reduces duplication, improves discoverability, maintains consistent test patterns.

### While Coding

- **Minimize diffs**: Change only what's necessary, preserve formatting and structure
- **Preserve behavior**: Don't break existing functionality or change semantics unintentionally
- **Build incrementally**: Run `dotnet build` after each logical change to catch errors early
- **Test continuously**: Run `dotnet test` frequently to verify correctness
- **Match style**: Follow the patterns in surrounding code exactly

### Validation

Before marking work complete, verify:

1. **Builds successfully**: `dotnet build Exceptionless.DateTimeExtensions.slnx` exits with code 0
2. **All tests pass**: `dotnet test Exceptionless.DateTimeExtensions.slnx` shows no failures
3. **No new warnings**: Check build output for new compiler warnings (warnings are treated as errors)
4. **API compatibility**: Public API changes are intentional and backward-compatible when possible
5. **Timezone correctness**: Verify explicit timezones are preserved and rounding respects offset
6. **Breaking changes flagged**: Clearly identify any breaking changes for review

### Error Handling

- **Validate inputs**: Check for null, empty strings, invalid ranges at method entry
- **Fail fast**: Throw exceptions immediately for invalid arguments (don't propagate bad data)
- **Meaningful messages**: Include parameter names and expected values in exception messages
- **TryParse pattern**: Always provide a `TryParse` alternative that returns `bool` instead of throwing
- **Use guard clauses**: Early returns for invalid conditions, keep happy path unindented

## Testing

### Philosophy: Battle-Tested Code

Tests are not just validation—they're **executable documentation** and **design tools**. Well-tested code is:

- **Trustworthy**: Confidence to refactor and extend
- **Documented**: Tests show how the API should be used
- **Resilient**: Edge cases are covered before they become production bugs

### Framework

- **xUnit** as the primary testing framework
- **Foundatio.Xunit** provides `TestWithLoggingBase` for test output logging
- Follow [Microsoft unit testing best practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

### Test-First Workflow

1. **Search for existing tests**: `dotnet test --filter "FullyQualifiedName~MethodYouAreChanging"`
2. **Extend existing test classes**: Add new `[Fact]` or `[Theory]` cases to existing files
3. **Write the failing test first**: Verify it fails for the right reason
4. **Implement minimal code**: Just enough to pass the test
5. **Add edge case tests**: Null inputs, timezone boundaries, leap years, DST transitions, overflow
6. **Run full test suite**: Ensure no regressions

### Test Principles (FIRST)

- **Fast**: Tests execute quickly
- **Isolated**: No dependencies on external services or execution order
- **Repeatable**: Consistent results every run
- **Self-checking**: Tests validate their own outcomes
- **Timely**: Write tests alongside code

### Naming Convention

Use the pattern: `MethodName_StateUnderTest_ExpectedBehavior`

Examples:

- `Parse_WithNowPlusOneHour_ReturnsOffsetDateTime`
- `TryParse_WithInvalidExpression_ReturnsFalse`
- `StartOfDay_WithDateTimeOffset_PreservesTimezone`

### Test Structure

Follow the AAA (Arrange-Act-Assert) pattern:

```csharp
[Fact]
public void Parse_WithExplicitUtcDate_PreservesTimezone()
{
    // Arrange
    var expression = "2025-01-01T01:25:35Z||+3d/d";
    var baseTime = DateTimeOffset.UtcNow;

    // Act
    var result = DateMath.Parse(expression, baseTime);

    // Assert
    Assert.Equal(TimeSpan.Zero, result.Offset);
    Assert.Equal(new DateTimeOffset(2025, 1, 4, 0, 0, 0, TimeSpan.Zero), result);
}
```

### Parameterized Tests

Use `[Theory]` with `[InlineData]` for multiple scenarios:

```csharp
[Theory]
[InlineData("1s", 1000)]
[InlineData("1m", 60000)]
[InlineData("1h", 3600000)]
[InlineData("1d", 86400000)]
public void Parse_WithValidTimeUnit_ReturnsExpectedMilliseconds(string input, double expectedMs)
{
    var result = TimeUnit.Parse(input);
    Assert.Equal(expectedMs, result.TotalMilliseconds);
}
```

### Test Organization

- Mirror the main code structure (e.g., `FormatParsers/` tests for `src/.../FormatParsers/`)
- Use `FormatParserTestsBase` and `PartParserTestsBase` for parser tests
- Inject `ITestOutputHelper` for test logging via `TestWithLoggingBase`

### Running Tests

```bash
# All tests
dotnet test Exceptionless.DateTimeExtensions.slnx

# Specific test file
dotnet test --filter "FullyQualifiedName~DateMathTests"

# With logging
dotnet test --logger "console;verbosity=detailed"
```

## Debugging

1. **Reproduce** with minimal steps—write a failing test
2. **Understand** the root cause before fixing (especially for timezone and parsing issues)
3. **Test** the fix thoroughly with multiple timezone scenarios
4. **Document** non-obvious fixes in code if needed

## Resources

- [README.md](README.md) - Overview and usage examples
- [NuGet Package](https://www.nuget.org/packages/Exceptionless.DateTimeExtensions/)
- [Elasticsearch Date Math Reference](https://www.elastic.co/guide/en/elasticsearch/reference/current/common-options.html#date-math)
