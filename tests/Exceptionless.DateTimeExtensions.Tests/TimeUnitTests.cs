using System;
using System.Collections.Generic;
using Xunit;

namespace Exceptionless.DateTimeExtensions.Tests;

public class TimeUnitTests
{
    public static IEnumerable<object[]> TestData =>
    [
        ["1000 nanos", new TimeSpan(10)],
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
        [" -1y ", new TimeSpan((int)(-1 * TimeSpanExtensions.AvgDaysInAYear), 0, 0, 0)]
    ];

    [Theory]
    [MemberData(nameof(TestData))]
    public void Parse_ValidInput_ReturnsExpectedTimeSpan(string value, TimeSpan expected)
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
    public void Parse_InvalidInput_ThrowsException(string value)
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
    public void TryParse_VariousInputs_ReturnsExpectedResult(string value, bool expected)
    {
        bool success = TimeUnit.TryParse(value, out var result);
        Assert.Equal(expected, success);
    }

    [Fact]
    public void Parse_UppercaseM_ParsesAsMonths()
    {
        // Arrange
        var input = "1M";
        var expectedDays = (int)TimeSpanExtensions.AvgDaysInAMonth;
        var expected = new TimeSpan(expectedDays, 0, 0, 0);

        // Act
        var result = TimeUnit.Parse(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_LowercaseM_ParsesAsMinutes()
    {
        // Arrange
        var input = "1m";
        var expected = new TimeSpan(0, 1, 0);

        // Act
        var result = TimeUnit.Parse(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_UppercaseAndLowercaseM_ProduceDifferentResults()
    {
        // Act
        var monthResult = TimeUnit.Parse("1M");
        var minuteResult = TimeUnit.Parse("1m");

        // Assert
        Assert.NotEqual(monthResult, minuteResult);
    }

    [Theory]
    [InlineData("1y")]
    [InlineData("2y")]
    [InlineData("-1y")]
    public void Parse_YearUnit_ReturnsExpectedDays(string input)
    {
        // Act
        var result = TimeUnit.Parse(input);
        var expectedDays = int.Parse(input.Substring(0, input.Length - 1)) * TimeSpanExtensions.AvgDaysInAYear;

        // Assert
        Assert.True(Math.Abs(result.TotalDays - expectedDays) < 1,
            $"Year conversion should be close to {expectedDays} days, got {result.TotalDays}");
    }

    [Theory]
    [InlineData("1M")]
    [InlineData("3M")]
    [InlineData("-1M")]
    public void Parse_MonthUnit_ReturnsExpectedDays(string input)
    {
        // Act
        var result = TimeUnit.Parse(input);
        var expectedDays = int.Parse(input.Substring(0, input.Length - 1)) * TimeSpanExtensions.AvgDaysInAMonth;

        // Assert
        Assert.True(Math.Abs(result.TotalDays - expectedDays) < 1,
            $"Month conversion should be close to {expectedDays} days, got {result.TotalDays}");
    }

    [Theory]
    [InlineData("1w", 7)]
    [InlineData("2w", 14)]
    [InlineData("4w", 28)]
    [InlineData("-1w", -7)]
    public void Parse_WeekUnit_ReturnsExpectedDays(string input, int expectedDays)
    {
        // Act
        var result = TimeUnit.Parse(input);

        // Assert
        Assert.Equal(expectedDays, result.TotalDays);
    }

    [Theory]
    [InlineData("10D")]
    [InlineData("1D")]
    [InlineData("7D")]
    public void Parse_UppercaseD_ThrowsException(string input)
    {
        // Arrange - Uppercase D is NOT a valid unit per Elasticsearch spec

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => TimeUnit.Parse(input));
    }

    [Theory]
    [InlineData("10D", false)]
    [InlineData("7D", false)]
    [InlineData("10d", true)]
    [InlineData("7d", true)]
    public void TryParse_UppercaseAndLowercaseD_ProduceDifferentResults(string input, bool expectedSuccess)
    {
        // Act
        bool success = TimeUnit.TryParse(input, out _);

        // Assert
        Assert.Equal(expectedSuccess, success);
    }
}
