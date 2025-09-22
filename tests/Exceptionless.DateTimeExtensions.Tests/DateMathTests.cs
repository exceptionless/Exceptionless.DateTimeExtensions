using System;
using Foundatio.Xunit;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

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
    [InlineData("now+1H", 1)] // Case insensitive
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
    [InlineData("2023-01-01")] // Missing ||
    [InlineData("||+1d")] // Missing anchor
    [InlineData("now/x")] // Invalid rounding unit
    [InlineData("2023-13-01||")] // Invalid month
    [InlineData("2023-01-32||")] // Invalid day
    [InlineData("2001.02.01||")] // Dotted format no longer supported
    [InlineData("now/d+1h")] // Rounding must be final operation
    [InlineData("now/d/d")] // Multiple rounding operations
    [InlineData("now+1h/d+2m")] // Rounding in middle of operations
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
    [InlineData("2023-01-01")] // Missing ||
    [InlineData("||+1d")] // Missing anchor
    [InlineData("2001.02.01||")] // Dotted format no longer supported
    [InlineData("now/d+1h")] // Rounding must be final operation
    [InlineData("now/d/d")] // Multiple rounding operations
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

    [Theory]
    [InlineData("now+1h", false)]
    [InlineData("now-1d/d", true)]
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
}
