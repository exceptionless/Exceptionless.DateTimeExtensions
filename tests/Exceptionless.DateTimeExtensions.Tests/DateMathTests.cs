using System;
using Foundatio.Xunit;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Exceptionless.DateTimeExtensions.Tests;

/// <summary>
/// Comprehensive tests for the DateMath utility class, covering all parsing scenarios,
/// edge cases, timezone handling, and error conditions.
/// </summary>
public class DateMathTests : TestWithLoggingBase
{
    private readonly DateTimeOffset _baseTime = new(2023, 6, 15, 14, 30, 45, 123, TimeSpan.FromHours(5));

    public DateMathTests(ITestOutputHelper output) : base(output)
    {
    }

    [Theory]
    [InlineData("now", false)]
    [InlineData("now", true)]
    public void Parse_Now_ReturnsBaseTime(string expression, bool isUpperLimit)
    {
        _logger.LogDebug("Testing Parse with expression: '{Expression}', BaseTime: {BaseTime}, IsUpperLimit: {IsUpperLimit}",
            expression, _baseTime, isUpperLimit);

        var result = DateMath.Parse(expression, _baseTime, isUpperLimit);

        _logger.LogDebug("Parse result: {Result}", result);
        Assert.Equal(_baseTime, result);
    }

    [Theory]
    [InlineData("now+1h", 1)]
    [InlineData("now+2h", 2)]
    [InlineData("now+24h", 24)]
    [InlineData("now+1H", 1)] // Both h and H are valid Elastic units for hours
    [InlineData("now-1h", -1)]
    [InlineData("now-12h", -12)]
    public void Parse_HourOperations_ReturnsCorrectResult(string expression, int hours)
    {
        var expected = _baseTime.AddHours(hours);
        _logger.LogDebug("Testing Parse with expression: '{Expression}', BaseTime: {BaseTime}, Expected: {Expected}",
            expression, _baseTime, expected);

        var result = DateMath.Parse(expression, _baseTime);

        _logger.LogDebug("Parse result: {Result}", result);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("now+1d", 1)]
    [InlineData("now+7d", 7)]
    [InlineData("now-1d", -1)]
    [InlineData("now-30d", -30)]
    public void Parse_DayOperations_ReturnsCorrectResult(string expression, int days)
    {
        var expected = _baseTime.AddDays(days);
        _logger.LogDebug("Testing Parse with expression: '{Expression}', BaseTime: {BaseTime}, Expected: {Expected}",
            expression, _baseTime, expected);

        var result = DateMath.Parse(expression, _baseTime);

        _logger.LogDebug("Parse result: {Result}", result);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("now+1M", 1)]
    [InlineData("now+6M", 6)]
    [InlineData("now-1M", -1)]
    [InlineData("now-12M", -12)]
    public void Parse_MonthOperations_ReturnsCorrectResult(string expression, int months)
    {
        var expected = _baseTime.AddMonths(months);
        _logger.LogDebug("Testing Parse with expression: '{Expression}', BaseTime: {BaseTime}, Expected: {Expected}",
            expression, _baseTime, expected);

        var result = DateMath.Parse(expression, _baseTime);

        _logger.LogDebug("Parse result: {Result}", result);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("now+1y", 1)]
    [InlineData("now+5y", 5)]
    [InlineData("now-1y", -1)]
    [InlineData("now-10y", -10)]
    public void Parse_YearOperations_ReturnsCorrectResult(string expression, int years)
    {
        var expected = _baseTime.AddYears(years);
        _logger.LogDebug("Testing Parse with expression: '{Expression}', BaseTime: {BaseTime}, Expected: {Expected}",
            expression, _baseTime, expected);

        var result = DateMath.Parse(expression, _baseTime);

        _logger.LogDebug("Parse result: {Result}", result);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("now+1w", 7)]
    [InlineData("now+2w", 14)]
    [InlineData("now-1w", -7)]
    [InlineData("now-4w", -28)]
    public void Parse_WeekOperations_ReturnsCorrectResult(string expression, int days)
    {
        var expected = _baseTime.AddDays(days);
        _logger.LogDebug("Testing Parse with expression: '{Expression}', BaseTime: {BaseTime}, Expected: {Expected}",
            expression, _baseTime, expected);

        var result = DateMath.Parse(expression, _baseTime);

        _logger.LogDebug("Parse result: {Result}", result);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("now+1m", 1)]
    [InlineData("now+30m", 30)]
    [InlineData("now-1m", -1)]
    [InlineData("now-60m", -60)]
    public void Parse_MinuteOperations_ReturnsCorrectResult(string expression, int minutes)
    {
        var expected = _baseTime.AddMinutes(minutes);
        _logger.LogDebug("Testing Parse with expression: '{Expression}', BaseTime: {BaseTime}, Expected: {Expected}",
            expression, _baseTime, expected);

        var result = DateMath.Parse(expression, _baseTime);

        _logger.LogDebug("Parse result: {Result}", result);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("now+1s", 1)]
    [InlineData("now+30s", 30)]
    [InlineData("now-1s", -1)]
    [InlineData("now-3600s", -3600)]
    public void Parse_SecondOperations_ReturnsCorrectResult(string expression, int seconds)
    {
        var expected = _baseTime.AddSeconds(seconds);
        _logger.LogDebug("Testing Parse with expression: '{Expression}', BaseTime: {BaseTime}, Expected: {Expected}",
            expression, _baseTime, expected);

        var result = DateMath.Parse(expression, _baseTime);

        _logger.LogDebug("Parse result: {Result}", result);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("now/d", false)] // Start of day
    [InlineData("now/d", true)] // End of day
    [InlineData("now/h", false)] // Start of hour
    [InlineData("now/h", true)] // End of hour
    [InlineData("now/m", false)] // Start of minute
    [InlineData("now/m", true)] // End of minute
    public void Parse_RoundingOperations_ReturnsCorrectResult(string expression, bool isUpperLimit)
    {
        _logger.LogDebug("Testing Parse with rounding expression: '{Expression}', BaseTime: {BaseTime}, IsUpperLimit: {IsUpperLimit}",
            expression, _baseTime, isUpperLimit);

        var result = DateMath.Parse(expression, _baseTime, isUpperLimit);

        _logger.LogDebug("Parse result: {Result}", result);

        if (expression.EndsWith("/d"))
        {
            if (isUpperLimit)
            {
                // End of day: 23:59:59.999
                var expectedEnd = new DateTimeOffset(_baseTime.Year, _baseTime.Month, _baseTime.Day, 23, 59, 59, 999, _baseTime.Offset);
                Assert.Equal(expectedEnd, result);
            }
            else
            {
                // Start of day: 00:00:00.000
                var expectedStart = new DateTimeOffset(_baseTime.Year, _baseTime.Month, _baseTime.Day, 0, 0, 0, 0, _baseTime.Offset);
                Assert.Equal(expectedStart, result);
            }
        }
        else if (expression.EndsWith("/h"))
        {
            var hourStart = new DateTimeOffset(_baseTime.Year, _baseTime.Month, _baseTime.Day,
                _baseTime.Hour, 0, 0, 0, _baseTime.Offset);

            Assert.Equal(isUpperLimit ? hourStart.AddHours(1).AddMilliseconds(-1) : hourStart, result);
        }
        else if (expression.EndsWith("/m"))
        {
            var minuteStart = new DateTimeOffset(_baseTime.Year, _baseTime.Month, _baseTime.Day,
                _baseTime.Hour, _baseTime.Minute, 0, 0, _baseTime.Offset);

            Assert.Equal(isUpperLimit ? minuteStart.AddMinutes(1).AddMilliseconds(-1) : minuteStart, result);
        }
    }

    [Theory]
    [InlineData("now+1d+2h")]
    [InlineData("now-1d+12h")]
    [InlineData("now+1M+1d")]
    [InlineData("now+1y-1M")]
    public void Parse_MultipleOperations_ReturnsCorrectResult(string expression)
    {
        _logger.LogDebug("Testing Parse with multiple operations: '{Expression}', BaseTime: {BaseTime}",
            expression, _baseTime);

        var result = DateMath.Parse(expression, _baseTime);

        _logger.LogDebug("Parse result: {Result}", result);

        // For simple combinations, we can verify exact results
        if (expression == "now+1d+2h")
        {
            var expected = _baseTime.AddDays(1).AddHours(2);
            Assert.Equal(expected, result);
        }
        else if (expression == "now-1d+12h")
        {
            var expected = _baseTime.AddDays(-1).AddHours(12);
            Assert.Equal(expected, result);
        }
        else
        {
            // For month/year operations, just verify the result is reasonable
            Assert.NotEqual(_baseTime, result);
        }
    }

    [Theory]
    [InlineData("2023-06-15||")]
    [InlineData("2023-06-15T10:30:00||")]
    [InlineData("2023-06-15T10:30:00.123||")]
    public void Parse_ExplicitDateFormats_ReturnsCorrectResult(string expression)
    {
        _logger.LogDebug("Testing Parse with explicit date format: '{Expression}', BaseTime: {BaseTime}",
            expression, _baseTime);

        var result = DateMath.Parse(expression, _baseTime);

        _logger.LogDebug("Parse result: {Result}", result);

        // All should resolve to June 15, 2023 in the base time's timezone
        Assert.Equal(2023, result.Year);
        Assert.Equal(6, result.Month);
        Assert.Equal(15, result.Day);
        Assert.Equal(_baseTime.Offset, result.Offset);
    }

    [Theory]
    [InlineData("2023-06-15T10:30:00Z||", 0)]
    [InlineData("2023-06-15T10:30:00+02:00||", 2)]
    [InlineData("2023-06-15T10:30:00-05:00||", -5)]
    [InlineData("2023-06-15T10:30:00+09:30||", 9.5)]
    public void Parse_ExplicitTimezones_PreservesTimezone(string expression, double offsetHours)
    {
        _logger.LogDebug("Testing Parse with explicit timezone: '{Expression}', BaseTime: {BaseTime}, Expected offset hours: {OffsetHours}",
            expression, _baseTime, offsetHours);

        var result = DateMath.Parse(expression, _baseTime);
        var expectedOffset = TimeSpan.FromHours(offsetHours);

        _logger.LogDebug("Parse result: {Result}, Expected offset: {ExpectedOffset}", result, expectedOffset);

        Assert.Equal(2023, result.Year);
        Assert.Equal(6, result.Month);
        Assert.Equal(15, result.Day);
        Assert.Equal(10, result.Hour);
        Assert.Equal(30, result.Minute);
        Assert.Equal(expectedOffset, result.Offset);
    }

    [Theory]
    [InlineData("2023-06-15||+1M")]
    [InlineData("2023-06-15T10:30:00||+2d")]
    [InlineData("2023-06-15T10:30:00Z||+1h")]
    [InlineData("2023-06-15T10:30:00+02:00||-1d/d")]
    public void Parse_ExplicitDateWithOperations_ReturnsCorrectResult(string expression)
    {
        _logger.LogDebug("Testing Parse with explicit date and operations: '{Expression}', BaseTime: {BaseTime}",
            expression, _baseTime);

        var result = DateMath.Parse(expression, _baseTime);

        _logger.LogDebug("Parse result: {Result}", result);

        // Verify it's not the base time and the operation was applied
        Assert.NotEqual(_baseTime, result);

        if (expression.Contains("+1M"))
        {
            Assert.Equal(7, result.Month); // June + 1 month = July
        }
        else if (expression.Contains("+2d"))
        {
            Assert.Equal(17, result.Day); // 15 + 2 days = 17
        }
        else if (expression.Contains("+1h"))
        {
            Assert.Equal(11, result.Hour); // 10 + 1 hour = 11
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid")]
    [InlineData("now+1x")] // Invalid unit
    [InlineData("||+1d")] // Missing anchor
    [InlineData("now/x")] // Invalid rounding unit
    [InlineData("2023-13-01||")] // Invalid month
    [InlineData("2023-01-32||")] // Invalid day
    [InlineData("2001.02.01||")] // Dotted format no longer supported
    [InlineData("now/d+1h")] // Rounding must be final operation
    [InlineData("now/d/d")] // Multiple rounding operations
    [InlineData("now+1h/d+2m")] // Rounding in middle of operations
    [InlineData("Now")] // 'now' must be lowercase per Elastic spec
    [InlineData("NOW")]
    [InlineData("NOW+1h")]
    [InlineData("Now-1d/d")]
    public void Parse_InvalidExpressions_ThrowsArgumentException(string expression)
    {
        _logger.LogDebug("Testing Parse with invalid expression: '{Expression}', expecting ArgumentException", expression);

        var exception = Assert.Throws<ArgumentException>(() => DateMath.Parse(expression, _baseTime));

        _logger.LogDebug("Exception thrown as expected: {Message}", exception.Message);
        Assert.Contains("Invalid date math expression", exception.Message);
    }

    [Fact]
    public void Parse_NullExpression_ThrowsArgumentException()
    {
        _logger.LogDebug("Testing Parse with null expression, expecting ArgumentException");

        var exception = Assert.Throws<ArgumentException>(() => DateMath.Parse(null!, _baseTime));

        _logger.LogDebug("Exception thrown as expected: {Message}", exception.Message);
    }

    [Theory]
    [InlineData("now")]
    [InlineData("now+1h")]
    [InlineData("now-1d/d")]
    [InlineData("2023-06-15")]
    [InlineData("2023-06-15||")]
    [InlineData("2023-06-15||+1M/d")]
    [InlineData("2025-01-01T01:25:35Z||+3d/d")]
    public void TryParse_ValidExpressions_ReturnsTrueAndCorrectResult(string expression)
    {
        _logger.LogDebug("Testing TryParse with valid expression: '{Expression}', BaseTime: {BaseTime}",
            expression, _baseTime);

        bool success = DateMath.TryParse(expression, _baseTime, false, out var result);

        _logger.LogDebug("TryParse success: {Success}, Result: {Result}", success, result);

        Assert.True(success);
        Assert.NotEqual(default, result);

        // Verify TryParse and Parse return the same result
        var parseResult = DateMath.Parse(expression, _baseTime, false);
        Assert.Equal(parseResult, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("now+")]
    [InlineData("||+1d")] // Missing anchor
    [InlineData("2001.02.01||")] // Dotted format no longer supported
    [InlineData("now/d+1h")] // Rounding must be final operation
    [InlineData("now/d/d")] // Multiple rounding operations
    [InlineData("Now+1h")] // 'now' must be lowercase
    [InlineData("NOW-1d")]
    public void TryParse_InvalidExpressions_ReturnsFalse(string expression)
    {
        _logger.LogDebug("Testing TryParse with invalid expression: '{Expression}', expecting false", expression);

        bool success = DateMath.TryParse(expression, _baseTime, false, out var result);

        _logger.LogDebug("TryParse success: {Success}, Result: {Result}", success, result);

        Assert.False(success);
        Assert.Equal(default, result);
    }

    [Fact]
    public void TryParse_NullExpression_ReturnsFalse()
    {
        _logger.LogDebug("Testing TryParse with null expression, expecting false");

        bool success = DateMath.TryParse(null!, _baseTime, false, out var result);

        _logger.LogDebug("TryParse success: {Success}, Result: {Result}", success, result);

        Assert.False(success);
        Assert.Equal(default, result);
    }

    [Fact]
    public void TryParse_FallbackExplicitDate_AppliesBaseOffset()
    {
        const string expression = "2023-04-01";
        _logger.LogDebug("Testing TryParse fallback with expression: '{Expression}', BaseTime: {BaseTime}", expression, _baseTime);

        bool success = DateMath.TryParse(expression, _baseTime, false, out var result);

        _logger.LogDebug("TryParse success: {Success}, Result: {Result}", success, result);

        Assert.True(success);
        var expected = new DateTimeOffset(2023, 4, 1, 0, 0, 0, _baseTime.Offset);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TryParse_FallbackExplicitDateUpperLimit_AdjustsToEndOfDay()
    {
        const string expression = "2023-07-10";
        _logger.LogDebug("Testing TryParse fallback upper limit with expression: '{Expression}', BaseTime: {BaseTime}", expression, _baseTime);

        bool success = DateMath.TryParse(expression, _baseTime, true, out var result);

        _logger.LogDebug("TryParse success: {Success}, Result: {Result}", success, result);

        Assert.True(success);
        var expected = new DateTimeOffset(2023, 7, 10, 23, 59, 59, 999, _baseTime.Offset);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TryParse_FallbackExplicitDateWithTimezone_PreservesOffset()
    {
        const string expression = "2023-05-05T18:45:00-07:00";
        _logger.LogDebug("Testing TryParse fallback with explicit offset expression: '{Expression}'", expression);

        bool success = DateMath.TryParse(expression, _baseTime, false, out var result);

        _logger.LogDebug("TryParse success: {Success}, Result: {Result}", success, result);

        Assert.True(success);
        Assert.Equal(new DateTimeOffset(2023, 5, 5, 18, 45, 0, TimeSpan.FromHours(-7)), result);
    }

    [Fact]
    public void TryParse_FallbackExplicitDateWithTimeZoneInfo_UsesProvidedOffset()
    {
        const string expression = "2023-09-15";
        var customZone = TimeZoneInfo.CreateCustomTimeZone("TestPlusThree", TimeSpan.FromHours(3), "Test +3", "Test +3");
        _logger.LogDebug("Testing TryParse fallback with TimeZoneInfo: '{Expression}', TimeZone: {TimeZone}", expression, customZone);

        bool success = DateMath.TryParse(expression, customZone, false, out var result);

        _logger.LogDebug("TryParse success: {Success}, Result: {Result}", success, result);

        Assert.True(success);
        Assert.Equal(new DateTimeOffset(2023, 9, 15, 0, 0, 0, customZone.BaseUtcOffset), result);
    }

    [Theory]
    [InlineData("now+1h", false)]
    [InlineData("now-1d/d", true)]
    [InlineData("2023-06-15", false)]
    [InlineData("2023-06-15||+1M", false)]
    [InlineData("2025-01-01T01:25:35Z||+3d/d", true)]
    public void Parse_And_TryParse_ReturnSameResults(string expression, bool isUpperLimit)
    {
        _logger.LogDebug("Testing Parse vs TryParse consistency for expression: '{Expression}', IsUpperLimit: {IsUpperLimit}",
            expression, isUpperLimit);

        var parseResult = DateMath.Parse(expression, _baseTime, isUpperLimit);
        bool tryParseSuccess = DateMath.TryParse(expression, _baseTime, isUpperLimit, out var tryParseResult);

        _logger.LogDebug("Parse result: {ParseResult}, TryParse success: {TryParseSuccess}, TryParse result: {TryParseResult}",
            parseResult, tryParseSuccess, tryParseResult);

        Assert.True(tryParseSuccess);
        Assert.Equal(parseResult, tryParseResult);
    }

    [Theory]
    [InlineData("now/d")]
    [InlineData("now/h")]
    [InlineData("now/m")]
    [InlineData("now+1d/d")]
    [InlineData("now-1M/d")]
    public void Parse_UpperLimitVsLowerLimit_ProducesDifferentResults(string expression)
    {
        _logger.LogDebug("Testing upper vs lower limit behavior for expression: '{Expression}'", expression);

        var lowerResult = DateMath.Parse(expression, _baseTime, false);
        var upperResult = DateMath.Parse(expression, _baseTime, true);

        _logger.LogDebug("Lower limit result: {LowerResult}, Upper limit result: {UpperResult}",
            lowerResult, upperResult);

        // Upper limit should be later than lower limit for rounding operations
        Assert.True(upperResult > lowerResult,
            $"Upper limit ({upperResult}) should be greater than lower limit ({lowerResult})");
    }

    [Fact]
    public void Parse_EdgeCase_LeapYear()
    {
        var leapYearDate = new DateTimeOffset(2024, 2, 28, 12, 0, 0, _baseTime.Offset);
        const string expression = "now+1d";

        _logger.LogDebug("Testing leap year edge case with date: {LeapYearDate}, expression: '{Expression}'",
            leapYearDate, expression);

        var result = DateMath.Parse(expression, leapYearDate);

        _logger.LogDebug("Parse result: {Result}", result);

        Assert.Equal(29, result.Day); // Should go to Feb 29 in leap year
        Assert.Equal(2, result.Month);
    }

    [Fact]
    public void Parse_EdgeCase_MonthOverflow()
    {
        var endOfMonth = new DateTimeOffset(2023, 1, 31, 12, 0, 0, _baseTime.Offset);
        const string expression = "now+1M";

        _logger.LogDebug("Testing month overflow edge case with date: {EndOfMonth}, expression: '{Expression}'",
            endOfMonth, expression);

        var result = DateMath.Parse(expression, endOfMonth);

        _logger.LogDebug("Parse result: {Result}", result);

        // January 31 + 1 month should go to February 28 (or 29 in leap year)
        Assert.Equal(2, result.Month);
        Assert.True(result.Day <= 29);
    }

    [Fact]
    public void Parse_EdgeCase_YearOverflow()
    {
        var endOfYear = new DateTimeOffset(2023, 12, 31, 23, 59, 59, _baseTime.Offset);
        const string expression = "now+1d";

        _logger.LogDebug("Testing year overflow edge case with date: {EndOfYear}, expression: '{Expression}'",
            endOfYear, expression);

        var result = DateMath.Parse(expression, endOfYear);

        _logger.LogDebug("Parse result: {Result}", result);

        Assert.Equal(2024, result.Year);
        Assert.Equal(1, result.Month);
        Assert.Equal(1, result.Day);
    }

    [Fact]
    public void Parse_ComplexExpression_MultipleOperationsWithRounding()
    {
        const string expression = "now+1M-2d+3h/h";

        _logger.LogDebug("Testing complex expression: '{Expression}', BaseTime: {BaseTime}", expression, _baseTime);

        var result = DateMath.Parse(expression, _baseTime, false);

        _logger.LogDebug("Parse result: {Result}", result);

        // Should be rounded to start of hour
        Assert.Equal(0, result.Minute);
        Assert.Equal(0, result.Second);
        Assert.Equal(0, result.Millisecond);

        // Should not equal base time
        Assert.NotEqual(_baseTime, result);
    }

    [Fact]
    public void ParseTimeZone_Now_ReturnsCurrentTimeInSpecifiedTimezone()
    {
        var utcTimeZone = TimeZoneInfo.Utc;
        const string expression = "now";

        _logger.LogDebug("Testing Parse with TimeZoneInfo for expression: '{Expression}', TimeZone: {TimeZone}",
            expression, utcTimeZone.Id);

        var result = DateMath.Parse(expression, utcTimeZone);

        _logger.LogDebug("Parse result: {Result}", result);

        // Should be close to current UTC time
        var utcNow = DateTimeOffset.UtcNow;
        Assert.True(Math.Abs((result - utcNow).TotalSeconds) < 5,
            $"Result {result} should be within 5 seconds of UTC now {utcNow}");
        Assert.Equal(TimeSpan.Zero, result.Offset); // Should be UTC
    }

    [Theory]
    [InlineData("UTC", 0)]
    [InlineData("US/Eastern", -5)] // EST offset (not considering DST for this test)
    [InlineData("US/Pacific", -8)] // PST offset (not considering DST for this test)
    public void ParseTimeZone_Now_ReturnsCorrectTimezone(string timeZoneId, int expectedOffsetHours)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        const string expression = "now";

        _logger.LogDebug("Testing Parse with TimeZoneInfo for expression: '{Expression}', TimeZone: {TimeZone}",
            expression, timeZone.Id);

        var result = DateMath.Parse(expression, timeZone);

        _logger.LogDebug("Parse result: {Result}, Expected offset hours: {ExpectedOffsetHours}", result, expectedOffsetHours);

        // Note: This test might need adjustment for DST, but it demonstrates the concept
        Assert.Equal(timeZone.GetUtcOffset(DateTime.UtcNow), result.Offset);
    }

    [Fact]
    public void ParseTimeZone_ExplicitDateWithoutTimezone_UsesSpecifiedTimezone()
    {
        var easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("US/Eastern");
        const string expression = "2023-06-15T14:30:00";

        _logger.LogDebug("Testing Parse with TimeZoneInfo for expression: '{Expression}', TimeZone: {TimeZone}",
            expression, easternTimeZone.Id);

        var result = DateMath.Parse(expression, easternTimeZone);

        _logger.LogDebug("Parse result: {Result}", result);

        Assert.Equal(2023, result.Year);
        Assert.Equal(6, result.Month);
        Assert.Equal(15, result.Day);
        Assert.Equal(14, result.Hour);
        Assert.Equal(30, result.Minute);
        Assert.Equal(0, result.Second);

        // Should use the timezone offset from Eastern Time
        var expectedOffset = easternTimeZone.GetUtcOffset(new DateTime(2023, 6, 15, 14, 30, 0));
        Assert.Equal(expectedOffset, result.Offset);
    }

    [Fact]
    public void ParseTimeZone_ExplicitDateWithTimezone_PreservesOriginalTimezone()
    {
        var pacificTimeZone = TimeZoneInfo.FindSystemTimeZoneById("US/Pacific");
        const string expression = "2023-06-15T14:30:00+05:00"; // Explicit +05:00 timezone

        _logger.LogDebug("Testing Parse with TimeZoneInfo for expression: '{Expression}', TimeZone: {TimeZone}",
            expression, pacificTimeZone.Id);

        var result = DateMath.Parse(expression, pacificTimeZone);

        _logger.LogDebug("Parse result: {Result}", result);

        Assert.Equal(2023, result.Year);
        Assert.Equal(6, result.Month);
        Assert.Equal(15, result.Day);
        Assert.Equal(14, result.Hour);
        Assert.Equal(30, result.Minute);
        Assert.Equal(0, result.Second);

        // Should preserve the original +05:00 timezone, not use Pacific
        Assert.Equal(TimeSpan.FromHours(5), result.Offset);
    }

    [Theory]
    [InlineData("now+1h", 1)]
    [InlineData("now+6h", 6)]
    [InlineData("now-2h", -2)]
    [InlineData("now+24h", 24)]
    public void ParseTimeZone_HourOperations_ReturnsCorrectResult(string expression, int hours)
    {
        var utcTimeZone = TimeZoneInfo.Utc;

        _logger.LogDebug("Testing Parse with TimeZoneInfo for expression: '{Expression}', TimeZone: {TimeZone}, Hours: {Hours}",
            expression, utcTimeZone.Id, hours);

        var result = DateMath.Parse(expression, utcTimeZone);
        var utcNow = DateTimeOffset.UtcNow;
        var expected = utcNow.AddHours(hours);

        _logger.LogDebug("Parse result: {Result}, Expected: approximately {Expected}", result, expected);

        // Should be close to expected time (within 5 seconds to account for execution time)
        Assert.True(Math.Abs((result - expected).TotalSeconds) < 5,
            $"Result {result} should be within 5 seconds of expected {expected}");
        Assert.Equal(TimeSpan.Zero, result.Offset); // Should be UTC
    }

    [Theory]
    [InlineData("now/d", false)]
    [InlineData("now/d", true)]
    [InlineData("now/h", false)]
    [InlineData("now/h", true)]
    [InlineData("now/M", false)]
    [InlineData("now/M", true)]
    public void ParseTimeZone_RoundingOperations_ReturnsCorrectResult(string expression, bool isUpperLimit)
    {
        var centralTimeZone = TimeZoneInfo.FindSystemTimeZoneById("US/Central");

        _logger.LogDebug("Testing Parse with TimeZoneInfo for expression: '{Expression}', TimeZone: {TimeZone}, IsUpperLimit: {IsUpperLimit}",
            expression, centralTimeZone.Id, isUpperLimit);

        var result = DateMath.Parse(expression, centralTimeZone, isUpperLimit);

        _logger.LogDebug("Parse result: {Result}", result);

        // Verify the result uses Central Time offset
        var expectedOffset = centralTimeZone.GetUtcOffset(DateTime.UtcNow);
        Assert.Equal(expectedOffset, result.Offset);

        // Verify rounding behavior
        if (expression.EndsWith("/d"))
        {
            if (isUpperLimit)
            {
                Assert.Equal(23, result.Hour);
                Assert.Equal(59, result.Minute);
                Assert.Equal(59, result.Second);
            }
            else
            {
                Assert.Equal(0, result.Hour);
                Assert.Equal(0, result.Minute);
                Assert.Equal(0, result.Second);
            }
        }
        else if (expression.EndsWith("/h"))
        {
            if (isUpperLimit)
            {
                Assert.Equal(59, result.Minute);
                Assert.Equal(59, result.Second);
            }
            else
            {
                Assert.Equal(0, result.Minute);
                Assert.Equal(0, result.Second);
            }
        }
    }

    [Fact]
    public void TryParseTimeZone_ValidExpression_ReturnsTrue()
    {
        var mountainTimeZone = TimeZoneInfo.FindSystemTimeZoneById("US/Mountain");
        const string expression = "now+2d";

        _logger.LogDebug("Testing TryParse with TimeZoneInfo for expression: '{Expression}', TimeZone: {TimeZone}",
            expression, mountainTimeZone.Id);

        bool success = DateMath.TryParse(expression, mountainTimeZone, false, out DateTimeOffset result);

        _logger.LogDebug("TryParse success: {Success}, Result: {Result}", success, result);

        Assert.True(success);
        Assert.NotEqual(default(DateTimeOffset), result);

        // Should use Mountain Time offset
        var expectedOffset = mountainTimeZone.GetUtcOffset(DateTime.UtcNow);
        Assert.Equal(expectedOffset, result.Offset);
    }

    [Fact]
    public void TryParseTimeZone_InvalidExpression_ReturnsFalse()
    {
        var utcTimeZone = TimeZoneInfo.Utc;
        const string expression = "invalid_expression";

        _logger.LogDebug("Testing TryParse with TimeZoneInfo for invalid expression: '{Expression}', TimeZone: {TimeZone}",
            expression, utcTimeZone.Id);

        bool success = DateMath.TryParse(expression, utcTimeZone, false, out DateTimeOffset result);

        _logger.LogDebug("TryParse success: {Success}, Result: {Result}", success, result);

        Assert.False(success);
        Assert.Equal(default(DateTimeOffset), result);
    }

    [Fact]
    public void ParseTimeZone_ComplexExpression_WorksCorrectly()
    {
        var utcTimeZone = TimeZoneInfo.Utc;
        const string expression = "now+1M-2d+3h/h";

        _logger.LogDebug("Testing Parse with TimeZoneInfo for complex expression: '{Expression}', TimeZone: {TimeZone}",
            expression, utcTimeZone.Id);

        var result = DateMath.Parse(expression, utcTimeZone, false);

        _logger.LogDebug("Parse result: {Result}", result);

        // Should be UTC
        Assert.Equal(TimeSpan.Zero, result.Offset);

        // Should be rounded to start of hour
        Assert.Equal(0, result.Minute);
        Assert.Equal(0, result.Second);
        Assert.Equal(0, result.Millisecond);
    }

    [Fact]
    public void ParseTimeZone_NullTimeZone_ThrowsArgumentNullException()
    {
        const string expression = "now";

        _logger.LogDebug("Testing Parse with null TimeZoneInfo for expression: '{Expression}'", expression);

        Assert.Throws<ArgumentNullException>(() => DateMath.Parse(expression, (TimeZoneInfo)null!));
    }

    [Fact]
    public void TryParseTimeZone_NullTimeZone_ThrowsArgumentNullException()
    {
        const string expression = "now";

        _logger.LogDebug("Testing TryParse with null TimeZoneInfo for expression: '{Expression}'", expression);

        Assert.Throws<ArgumentNullException>(() => DateMath.TryParse(expression, (TimeZoneInfo)null!, false, out _));
    }

    /// <summary>
    /// Per Elasticsearch docs, valid date-math units are case-sensitive:
    /// y, M, w, d, h, H, m, s. Uppercase D, Y, W, S are NOT valid units.
    /// https://www.elastic.co/docs/reference/elasticsearch/rest-apis/common-options
    /// </summary>
    [Theory]
    [InlineData("now-7D")]
    [InlineData("now-1D")]
    [InlineData("now-30D")]
    [InlineData("now+1D")]
    [InlineData("now-1Y")]
    [InlineData("now-1W")]
    [InlineData("now-1S")]
    [InlineData("now/D")]
    public void Parse_UppercaseInvalidUnits_ThrowsArgumentException(string expression)
    {
        // Arrange
        _logger.LogDebug("Testing Parse with invalid uppercase unit: '{Expression}', expecting ArgumentException", expression);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => DateMath.Parse(expression, _baseTime));

        _logger.LogDebug("Exception thrown as expected: {Message}", exception.Message);
    }

    [Theory]
    [InlineData("now-7D")]
    [InlineData("now-1D")]
    [InlineData("now+1D")]
    public void TryParse_UppercaseInvalidUnits_ReturnsFalse(string expression)
    {
        // Arrange
        _logger.LogDebug("Testing TryParse with invalid uppercase unit: '{Expression}', expecting false", expression);

        // Act
        bool success = DateMath.TryParse(expression, _baseTime, false, out var result);

        _logger.LogDebug("TryParse success: {Success}, Result: {Result}", success, result);

        // Assert
        Assert.False(success);
        Assert.Equal(default, result);
    }

    [Fact]
    public void Parse_UppercaseAndLowercaseM_ProduceDifferentResults()
    {
        // Arrange
        var minuteExpression = "now-1m";
        var monthExpression = "now+1M";

        _logger.LogDebug("Testing case-sensitive distinction: 'm' (minutes) vs 'M' (months), BaseTime: {BaseTime}",
            _baseTime);

        // Act
        var minuteResult = DateMath.Parse(minuteExpression, _baseTime);
        var monthResult = DateMath.Parse(monthExpression, _baseTime);

        _logger.LogDebug("now-1m result: {MinuteResult}, now+1M result: {MonthResult}", minuteResult, monthResult);

        // Assert
        Assert.Equal(_baseTime.AddMinutes(-1), minuteResult);
        Assert.Equal(_baseTime.AddMonths(1), monthResult);
    }

    [Fact]
    public void IsValidExpression_CaseSensitiveInputs_ValidatesCorrectly()
    {
        // Arrange
        _logger.LogDebug("Testing IsValidExpression with valid and invalid case-sensitive expressions");

        // Act & Assert - valid expressions
        Assert.True(DateMath.IsValidExpression("now-7d"));
        Assert.True(DateMath.IsValidExpression("now-1d/d"));

        // Act & Assert - uppercase D is not a valid unit
        Assert.False(DateMath.IsValidExpression("now-7D"));
        Assert.False(DateMath.IsValidExpression("now-1D/D"));

        // Act & Assert - 'now' must be lowercase
        Assert.False(DateMath.IsValidExpression("Now-7d"));
        Assert.False(DateMath.IsValidExpression("NOW-7d"));
    }
}
