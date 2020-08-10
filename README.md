# Exceptionless.DateTimeExtensions
[![Build status](https://github.com/Exceptionless/Exceptionless.DateTimeExtensions/workflows/Build/badge.svg)](https://github.com/Exceptionless/Exceptionless.DateTimeExtensions/actions)
[![NuGet Version](http://img.shields.io/nuget/v/Exceptionless.DateTimeExtensions.svg?style=flat)](https://www.nuget.org/packages/Exceptionless.DateTimeExtensions/) 
[![Slack Status](https://slack.exceptionless.com/badge.svg)](https://slack.exceptionless.com)
[![Donate](https://img.shields.io/badge/donorbox-donate-blue.svg)](https://donorbox.org/exceptionless) 

DateTimeRange, Business Day and various DateTime, DateTimeOffset, TimeSpan extension methods.

## Getting Started (Development)

[This package](https://www.nuget.org/packages/Exceptionless.DateTimeExtensions/) can be installed via the [NuGet package manager](https://docs.nuget.org/consume/Package-Manager-Dialog). If you need help, please contact us via in-app support or [open an issue](https://github.com/exceptionless/Exceptionless.DateTimeExtensions/issues/new). We’re always here to help if you have any questions!

1. You will need to have [Visual Studio Code](https://code.visualstudio.com/) installed.
2. Open the root folder.

## Using DateTimeExtensions

Below is a small sampling of the things you can accomplish with DateTimeExtensions, so check it out!

### Business Day

Quickly calculate if a datetime is within your hours of business. Check out our [unit tests](https://github.com/exceptionless/Exceptionless.DateTimeExtensions/blob/master/test/Exceptionless.DateTimeExtensions.Tests/BusinessDayTests.cs) for more usage samples.

```csharp
var date = DateTime.Now.StartOfDay().AddHours(8);

var day = new BusinessDay(date.Date.DayOfWeek,
    date.Subtract(TimeSpan.FromHours(1)).TimeOfDay,
    date.AddHours(1).TimeOfDay);

bool isDay = day.IsBusinessDay(date);
```

### DateTime Ranges

Quickly work with date ranges. . Check out our [unit tests](https://github.com/exceptionless/Exceptionless.DateTimeExtensions/blob/master/test/Exceptionless.DateTimeExtensions.Tests/DateTimeRangeTests.cs) for more usage samples.

```csharp
var range = DateTimeRange.Parse("yesterday", DateTime.Now);
if (range.Contains(DateTime.Now.Subtract(TimeSpan.FromHours(6)))) {
  //...
}
```

### TimeUnit

Quickly work with time units. . Check out our [unit tests](https://github.com/exceptionless/Exceptionless.DateTimeExtensions/blob/master/test/Exceptionless.DateTimeExtensions.Tests/TimeUnitTests.cs) for more usage samples.

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

Helper methods that makes working with DateTimes easier.  Check out the [source](https://github.com/exceptionless/Exceptionless.DateTimeExtensions/blob/master/src/Exceptionless.DateTimeExtensions/DateTimeExtensions.cs) for all of the extension methods you can use.

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

Helper methods that makes working with TimeSpans easier.  Check out the [source](https://github.com/exceptionless/Exceptionless.DateTimeExtensions/blob/master/src/Exceptionless.DateTimeExtensions/TimeSpanExtensions.cs) for all of the extension methods you can use.

```csharp
using Exceptionless.DateTimeExtensions;

var years = TimeSpan.FromHours(6).GetYears();
var totalYears = TimeSpan.FromHours(6).GetTotalYears();
```

## Thanks to all the people who have contributed

[![contributors](https://contributors-img.web.app/image?repo=exceptionless/Exceptionless.DateTimeExtensions)](https://github.com/exceptionless/Exceptionless.DateTimeExtensions/graphs/contributors)
