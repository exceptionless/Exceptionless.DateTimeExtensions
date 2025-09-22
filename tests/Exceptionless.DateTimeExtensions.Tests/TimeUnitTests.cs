using System;
using System.Collections.Generic;
using Xunit;

namespace Exceptionless.DateTimeExtensions.Tests;

public class TimeUnitTests
{
    public static IEnumerable<object[]> TestData => new[] {
        new object[] { "1000 nanos", new TimeSpan(10) },
        ["1000nanos", new TimeSpan(10)],
        ["1000 NANOS", new TimeSpan(10)],
        ["1000NANOS", new TimeSpan(10)],
        ["10micros", new TimeSpan(100)],
        ["10ms", new TimeSpan(0, 0, 0, 0, 10)],
        ["10s", new TimeSpan(0, 0, 10)],
        ["-10s", new TimeSpan(0, 0, -10)],
        ["10m", new TimeSpan(0, 10, 0)],
        ["10h", new TimeSpan(10, 0, 0)],
        ["10d", new TimeSpan(10, 0, 0, 0)],
        ["1w", new TimeSpan(7, 0, 0, 0)],
        ["2w", new TimeSpan(14, 0, 0, 0)],
        ["-1w", new TimeSpan(-7, 0, 0, 0)],
        ["1M", new TimeSpan((int)TimeSpanExtensions.AvgDaysInAMonth, 0, 0, 0)],
        ["2M", new TimeSpan((int)(2 * TimeSpanExtensions.AvgDaysInAMonth), 0, 0, 0)],
        ["-1M", new TimeSpan((int)(-1 * TimeSpanExtensions.AvgDaysInAMonth), 0, 0, 0)],
        ["1y", new TimeSpan((int)TimeSpanExtensions.AvgDaysInAYear, 0, 0, 0)],
        ["2y", new TimeSpan((int)(2 * TimeSpanExtensions.AvgDaysInAYear), 0, 0, 0)],
        ["-1y", new TimeSpan((int)(-1 * TimeSpanExtensions.AvgDaysInAYear), 0, 0, 0)],
        // Whitespace trimming tests
        [" 1y ", new TimeSpan((int)TimeSpanExtensions.AvgDaysInAYear, 0, 0, 0)],
        ["  2M  ", new TimeSpan((int)(2 * TimeSpanExtensions.AvgDaysInAMonth), 0, 0, 0)],
        ["\t3w\t", new TimeSpan(21, 0, 0, 0)],
        [" -1y ", new TimeSpan((int)(-1 * TimeSpanExtensions.AvgDaysInAYear), 0, 0, 0)],
    };

    [Theory]
    [MemberData(nameof(TestData))]
    public void CanParse(string value, TimeSpan expected)
    {
        Assert.Equal(expected, TimeUnit.Parse(value));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("1.234h")] // fractional time
    [InlineData("1234")] // missing unit
    [InlineData("12unknownunit")]
    [InlineData("12h.")]
    [InlineData("")] // empty string
    [InlineData("   ")] // whitespace only
    [InlineData("\t\t")] // tabs only
    [InlineData("1y@")] // special character after unit
    [InlineData("1M!")] // special character after unit
    [InlineData("1w#")] // special character after unit
    [InlineData("1@y")] // special character in middle
    public void VerifyParseFailure(string value)
    {
        Assert.ThrowsAny<Exception>(() => TimeUnit.Parse(value));
    }

    [Theory]
    [InlineData("1000 nanos", true)]
    [InlineData("1000nanos", true)]
    [InlineData("1000 NANOS", true)]
    [InlineData("1000NANOS", true)]
    [InlineData("10micros", true)]
    [InlineData("10ms", true)]
    [InlineData("10s", true)]
    [InlineData("-10s", true)]
    [InlineData("10m", true)]
    [InlineData("10h", true)]
    [InlineData("10d", true)]
    [InlineData("1w", true)]
    [InlineData("2w", true)]
    [InlineData("-1w", true)]
    [InlineData("1M", true)]
    [InlineData("2M", true)]
    [InlineData("-1M", true)]
    [InlineData("1y", true)]
    [InlineData("2y", true)]
    [InlineData("-1y", true)]
    // Whitespace tests
    [InlineData(" 1y ", true)]
    [InlineData("  2M  ", true)]
    [InlineData("\t3w\t", true)]
    [InlineData(" -1M ", true)]
    // Special character and edge case tests
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("\t\t", false)]
    [InlineData("1y@", false)]
    [InlineData("1M!", false)]
    [InlineData("1w#", false)]
    [InlineData("1@y", false)]
    [InlineData(null, false)]
    [InlineData("1.234h", false)] // fractional time
    [InlineData("1234", false)] // missing unit
    [InlineData("12unknownunit", false)]
    [InlineData("12h.", false)]
    [InlineData("Blah/Blahs", false)]
    public void VerifyTryParse(string value, bool expected)
    {
        bool success = TimeUnit.TryParse(value, out var result);
        Assert.Equal(expected, success);
    }

    [Fact]
    public void VerifyMonthsVsMinutesCaseSensitive()
    {
        // Uppercase M should be months
        var monthResult = TimeUnit.Parse("1M");
        var expectedMonthDays = (int)TimeSpanExtensions.AvgDaysInAMonth;
        Assert.Equal(new TimeSpan(expectedMonthDays, 0, 0, 0), monthResult);

        // Lowercase m should be minutes
        var minuteResult = TimeUnit.Parse("1m");
        Assert.Equal(new TimeSpan(0, 1, 0), minuteResult);

        // Verify they are different
        Assert.NotEqual(monthResult, minuteResult);
    }

    [Theory]
    [InlineData("1y", 365)]  // Approximately 365 days in a year
    [InlineData("1M", 30)]   // Approximately 30 days in a month  
    [InlineData("1w", 7)]    // Exactly 7 days in a week
    public void VerifyNewTimeUnitsConvertCorrectly(string input, int expectedApproxDays)
    {
        var result = TimeUnit.Parse(input);
        
        // For years and months, check approximate values due to fractional constants
        if (input.EndsWith("y"))
        {
            Assert.True(Math.Abs(result.TotalDays - TimeSpanExtensions.AvgDaysInAYear) < 1, 
                $"Year conversion should be close to {TimeSpanExtensions.AvgDaysInAYear} days, got {result.TotalDays}");
            Assert.True(Math.Abs(result.TotalDays - expectedApproxDays) < 10,
                $"Year conversion should be approximately {expectedApproxDays} days, got {result.TotalDays}");
        }
        else if (input.EndsWith("M"))
        {
            Assert.True(Math.Abs(result.TotalDays - TimeSpanExtensions.AvgDaysInAMonth) < 1,
                $"Month conversion should be close to {TimeSpanExtensions.AvgDaysInAMonth} days, got {result.TotalDays}");
            Assert.True(Math.Abs(result.TotalDays - expectedApproxDays) < 5,
                $"Month conversion should be approximately {expectedApproxDays} days, got {result.TotalDays}");
        }
        else if (input.EndsWith("w"))
        {
            Assert.Equal(expectedApproxDays, result.TotalDays);
        }
    }
}
