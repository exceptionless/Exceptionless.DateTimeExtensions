# Exceptionless.DateTimeExtensions

[![Build status](https://github.com/Exceptionless/Exceptionless.DateTimeExtensions/workflows/Build/badge.svg)](https://github.com/Exceptionless/Exceptionless.DateTimeExtensions/actions)
[![NuGet Version](http://img.shields.io/nuget/v/Exceptionless.DateTimeExtensions.svg?style=flat)](https://www.nuget.org/packages/Exceptionless.DateTimeExtensions/)
[![Discord](https://img.shields.io/discord/715744504891703319)](https://discord.gg/6HxgFCx)
[![Donate](https://img.shields.io/badge/donorbox-donate-blue.svg)](https://donorbox.org/exceptionless?recurring=true)

DateTimeRange, Business Day and various DateTime, DateTimeOffset, TimeSpan extension methods.

## Getting Started (Development)

[This package](https://www.nuget.org/packages/Exceptionless.DateTimeExtensions/) can be installed via the [NuGet package manager](https://docs.nuget.org/consume/Package-Manager-Dialog). If you need help, please contact us via in-app support or [open an issue](https://github.com/exceptionless/Exceptionless.DateTimeExtensions/issues/new). We’re always here to help if you have any questions!

1. You will need to have [Visual Studio Code](https://code.visualstudio.com/) installed.
2. Open the root folder.

## Using DateTimeExtensions

Below is a small sampling of the things you can accomplish with DateTimeExtensions, so check it out!

### Business Day

Quickly calculate if a datetime is within your hours of business. Check out our [unit tests](https://github.com/exceptionless/Exceptionless.DateTimeExtensions/blob/main/tests/Exceptionless.DateTimeExtensions.Tests/BusinessDayTests.cs) for more usage samples.

```csharp
var date = DateTime.Now.StartOfDay().AddHours(8);

var day = new BusinessDay(date.Date.DayOfWeek,
    date.Subtract(TimeSpan.FromHours(1)).TimeOfDay,
    date.AddHours(1).TimeOfDay);

bool isDay = day.IsBusinessDay(date);
```

### DateTime Ranges

Quickly work with date ranges with support for Elasticsearch-style date math expressions and bracket notation. Check out our [unit tests](https://github.com/exceptionless/Exceptionless.DateTimeExtensions/blob/main/tests/Exceptionless.DateTimeExtensions.Tests/DateTimeRangeTests.cs) for more usage samples.

```csharp
// Basic range parsing
var range = DateTimeRange.Parse("yesterday", DateTime.Now);
if (range.Contains(DateTime.Now.Subtract(TimeSpan.FromHours(6)))) {
  //...
}

// Elasticsearch Date Math support with proper timezone handling
var elasticRange = DateTimeRange.Parse("2025-01-01T01:25:35Z||+3d/d", DateTime.Now);
// Supports timezone-aware operations: Z (UTC), +05:00, -08:00

// Bracket notation support [start TO end]
var bracketRange = DateTimeRange.Parse("[2023-01-01 TO 2023-12-31]", DateTime.Now);

// Wildcard support for open-ended ranges
var wildcardRange = DateTimeRange.Parse("[2023-01-01 TO *]", DateTime.Now); // From date to infinity
```

#### Date Math Features

Supports full Elasticsearch date math syntax following [official specifications](https://www.elastic.co/guide/en/elasticsearch/reference/current/common-options.html#date-math):

- **Anchors**: `now`, explicit dates with `||` separator
- **Operations**: `+1d` (add), `-1h` (subtract), `/d` (round down)
- **Units**: `y` (years), `M` (months), `w` (weeks), `d` (days), `h`/`H` (hours), `m` (minutes), `s` (seconds)
- **Timezone Support**: Preserves explicit timezones (`Z`, `+05:00`, `-08:00`) or uses system timezone as fallback

Examples:

- `now+1h` - One hour from now
- `now-1d/d` - Start of yesterday
- `2025-01-01T01:25:35Z||+3d/d` - January 4th, 2025 (start of day) in UTC
- `2023-06-15T14:30:00+05:00||+1M-2d` - One month minus 2 days from the specified date/time in +05:00 timezone

### DateMath Utility

For applications that need standalone date math parsing without the range functionality, the `DateMath` utility class provides direct access to Elasticsearch date math expression parsing. Check out our [unit tests](https://github.com/exceptionless/Exceptionless.DateTimeExtensions/blob/main/tests/Exceptionless.DateTimeExtensions.Tests/DateMathTests.cs) for more usage samples.

```csharp
using Exceptionless.DateTimeExtensions;

// Parse date math expressions with standard .NET conventions
var baseTime = DateTimeOffset.Now;

// Parse method - throws ArgumentException on invalid input
var result = DateMath.Parse("now+1h", baseTime);
var rounded = DateMath.Parse("now-1d/d", baseTime, isUpperLimit: false); // Start of yesterday

// TryParse method - returns bool for success/failure
if (DateMath.TryParse("2023.06.15||+1M/d", baseTime, false, out var parsed)) {
    // Successfully parsed: June 15, 2023 + 1 month, rounded to start of day
    Console.WriteLine($"Parsed: {parsed:O}");
}

// Upper limit behavior affects rounding
var startOfDay = DateMath.Parse("now/d", baseTime, isUpperLimit: false); // 00:00:00
var endOfDay = DateMath.Parse("now/d", baseTime, isUpperLimit: true);     // 23:59:59.999

// Explicit dates with timezone preservation
var utcResult = DateMath.Parse("2025-01-01T01:25:35Z||+3d/d", baseTime);
var offsetResult = DateMath.Parse("2023-06-15T14:30:00+05:00||+1M", baseTime);
```

#### TimeZone-Aware DateMath

The `DateMath` utility also provides overloads that work directly with `TimeZoneInfo` for better timezone handling:

```csharp
using Exceptionless.DateTimeExtensions;

// Parse expressions using a specific timezone
var utcTimeZone = TimeZoneInfo.Utc;
var easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("US/Eastern");

// "now" will use current time in the specified timezone
var utcResult = DateMath.Parse("now+1h", utcTimeZone);
var easternResult = DateMath.Parse("now/d", easternTimeZone, isUpperLimit: false);

// TryParse with timezone
if (DateMath.TryParse("now+2d-3h", easternTimeZone, false, out var result)) {
    Console.WriteLine($"Eastern time result: {result:O}");
}

// Dates without explicit timezone use the provided TimeZoneInfo
var localDate = DateMath.Parse("2023-06-15T14:30:00||+1M", easternTimeZone);

// Dates with explicit timezone are preserved regardless of TimeZoneInfo parameter
var preservedTz = DateMath.Parse("2023-06-15T14:30:00+05:00||+1M", easternTimeZone);
// Result will still have +05:00 offset, not Eastern time offset
```

The `DateMath` utility supports the same comprehensive syntax as `DateTimeRange` but provides a simpler API for direct parsing operations.

### TimeUnit

Quickly work with time units. . Check out our [unit tests](https://github.com/exceptionless/Exceptionless.DateTimeExtensions/blob/main/tests/Exceptionless.DateTimeExtensions.Tests/TimeUnitTests.cs) for more usage samples.

```csharp
TimeSpan oneNanosecond = TimeUnit.Parse("1nanos");
TimeSpan oneMicrosecond = TimeUnit.Parse("1micros");
TimeSpan oneMillisecond = TimeUnit.Parse("1ms");
TimeSpan oneSecond = TimeUnit.Parse("1s");
TimeSpan oneMinute = TimeUnit.Parse("1m");
TimeSpan oneHour = TimeUnit.Parse("1h");
TimeSpan oneDay = TimeUnit.Parse("1d");
```

### DateTime Extension methods

Helper methods that makes working with DateTimes easier.  Check out the [source](https://github.com/exceptionless/Exceptionless.DateTimeExtensions/blob/main/src/Exceptionless.DateTimeExtensions/DateTimeExtensions.cs) for all of the extension methods you can use.

```csharp
using Exceptionless.DateTimeExtensions;

DateTime.Now.ToApproximateAgeString(); // "Just now"
var time = DateTime.Now.StartOfMinute();
var lastWeek = DateTime.Now.LastWeek();
var nextWeek = DateTime.Now.NextWeek();
```

### DateTimeOffset Extension methods

Helper methods that makes working with DateTimeOffsets easier.  Check out the [source](https://github.com/exceptionless/Exceptionless.DateTimeExtensions/blob/master/src/Exceptionless.DateTimeExtensions/DateTimeOffsetExtensions.cs) for all of the extension methods you can use.

```csharp
using Exceptionless.DateTimeExtensions;

DateTimeOffset.Now.ToApproximateAgeString(); // "Just now"
var startOfMonth = DateTimeOffset.Now.ToStartOfMonth();
var endOfMonth = DateTimeOffset.Now.ToEndOfMonth();
```

### Timespan Extension methods

Helper methods that makes working with TimeSpans easier.  Check out the [source](https://github.com/exceptionless/Exceptionless.DateTimeExtensions/blob/main/src/Exceptionless.DateTimeExtensions/TimeSpanExtensions.cs) for all of the extension methods you can use.

```csharp
using Exceptionless.DateTimeExtensions;

var years = TimeSpan.FromHours(6).GetYears();
var totalYears = TimeSpan.FromHours(6).GetTotalYears();
```

## Thanks to all the people who have contributed

[![contributors](https://contributors-img.web.app/image?repo=exceptionless/Exceptionless.DateTimeExtensions)](https://github.com/exceptionless/Exceptionless.DateTimeExtensions/graphs/contributors)
