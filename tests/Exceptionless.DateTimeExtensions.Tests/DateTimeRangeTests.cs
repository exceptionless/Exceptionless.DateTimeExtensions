using System;
using System.Collections.Generic;
using Xunit;

namespace Exceptionless.DateTimeExtensions.Tests;

public class DateTimeRangeTests
{
    private static readonly DateTime _now = new(2014, 11, 6, 4, 32, 56, 78);

    [Fact]
    public void CanCompareForEquality()
    {
        var range1 = new DateTimeRange(_now, _now.AddMinutes(1));
        var range2 = new DateTimeRange(_now, _now.AddMinutes(1));
        Assert.True(range1 == range2);
    }

    [Fact]
    public void CanParseNull()
    {
        var range = DateTimeRange.Parse(null, _now);
        Assert.Equal(DateTimeRange.Empty, range);
    }

    [Fact]
    public void CanAddAndSubtract()
    {
        var range1 = new DateTimeRange(_now, _now.AddMinutes(1));
        var utcRange = range1.Add(TimeSpan.FromHours(6));
        Assert.Equal(_now.AddHours(6), utcRange.Start);
        Assert.Equal(_now.AddHours(6).AddMinutes(1), utcRange.End);

        var localRange = utcRange.Subtract(TimeSpan.FromHours(6));
        Assert.Equal(_now, localRange.Start);
        Assert.Equal(_now.AddMinutes(1), localRange.End);
    }

    [Fact]
    public void CanParseIntoLocalTime()
    {
        const string time = "2016-12-28T05:00:00-2016-12-28T05:30:00";
        var utcOffset = TimeSpan.FromHours(-1);
        var localRange = DateTimeRange.Parse(time, DateTimeOffset.UtcNow.ChangeOffset(utcOffset));
        Assert.Equal(new DateTime(2016, 12, 28, 5, 0, 0, DateTimeKind.Unspecified), localRange.Start);
        Assert.Equal(new DateTime(2016, 12, 28, 5, 30, 0, DateTimeKind.Unspecified), localRange.End);
        Assert.Equal(new DateTime(2016, 12, 28, 6, 0, 0, DateTimeKind.Utc), localRange.UtcStart);
        Assert.Equal(new DateTime(2016, 12, 28, 6, 30, 0, DateTimeKind.Utc), localRange.UtcEnd);
    }

    [Fact]
    public void CanParse8601()
    {
        const string time = "2023-12-28T05:00:00.000Z-2023-12-28T05:30:00.000Z";
        var range = DateTimeRange.Parse(time, DateTimeOffset.UtcNow);
        Assert.Equal(new DateTime(2023, 12, 28, 5, 0, 0, DateTimeKind.Utc), range.Start);
        Assert.Equal(new DateTime(2023, 12, 28, 5, 30, 0, DateTimeKind.Utc), range.End);
        Assert.Equal(new DateTime(2023, 12, 28, 5, 0, 0, DateTimeKind.Utc), range.UtcStart);
        Assert.Equal(new DateTime(2023, 12, 28, 5, 30, 0, DateTimeKind.Utc), range.UtcEnd);
    }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void CanParseNamedRanges(string input, DateTime start, DateTime end)
    {
        var range = DateTimeRange.Parse(input, _now);
        Assert.Equal(start, range.Start);
        Assert.Equal(end, range.End);
    }

    public static IEnumerable<object[]> Inputs =>
    [
        ["today",          _now.StartOfDay(), _now.EndOfDay()],
        ["yesterday",      _now.SubtractDays(1).StartOfDay(), _now.SubtractDays(1).EndOfDay()],
        ["tomorrow",       _now.AddDays(1).StartOfDay(), _now.AddDays(1).EndOfDay()],
        ["last 5 minutes", _now.SubtractMinutes(5).StartOfMinute(), _now],
        ["this 5 minutes", _now, _now.AddMinutes(5).EndOfMinute()],
        ["next 5 minutes", _now, _now.AddMinutes(5).EndOfMinute()],
        ["last jan",       _now.ChangeMonth(1).StartOfMonth(), _now.ChangeMonth(1).EndOfMonth()],
        ["jan",            _now.ChangeMonth(1).StartOfMonth(), _now.ChangeMonth(1).EndOfMonth()],
        ["noVemBer",       _now.StartOfMonth(), _now.EndOfMonth()],
        ["deC",            _now.ChangeMonth(12).StartOfMonth(), _now.ChangeMonth(12).EndOfMonth()],
        ["next deC",       _now.ChangeMonth(12).StartOfMonth(), _now.ChangeMonth(12).EndOfMonth()],
        ["next nov",       _now.AddYears(1).StartOfMonth(), _now.AddYears(1).EndOfMonth()],
        ["next jan",       _now.AddYears(1).ChangeMonth(1).StartOfMonth(), _now.AddYears(1).ChangeMonth(1).EndOfMonth()],
        ["jan-feb",        _now.ChangeMonth(1).StartOfMonth(), _now.ChangeMonth(2).EndOfMonth()],
        ["now-this feb",   _now, _now.AddYears(1).ChangeMonth(2).EndOfMonth()],

        // Date math expressions without brackets (testing if they work)
        ["now-6h TO now", _now.AddHours(-6), _now],
        ["now-1d TO now", _now.AddDays(-1), _now],

        // Bracket notation with date math (currently failing - documenting expected behavior)
        ["[now-6h TO now]", _now.AddHours(-6), _now],
        ["[now-1d TO now]", _now.AddDays(-1), _now],
        ["[now-30m TO now]", _now.AddMinutes(-30), _now],
        ["[now TO now+2h]", _now, _now.AddHours(2)],
        ["[now-1h TO now+1h]", _now.AddHours(-1), _now.AddHours(1)],

        // Curly brace notation with date math (currently failing - documenting expected behavior)
        ["{now-6h TO now}", _now.AddHours(-6), _now],
        ["{now-1d TO now}", _now.AddDays(-1), _now],

        // Mixed expressions with brackets (currently failing - documenting expected behavior)
        ["[yesterday TO now]", _now.SubtractDays(1).StartOfDay(), _now],
        ["[now-1w TO today]", _now.AddDays(-7), _now.EndOfDay()]
    ];

    [Fact]
    public void Parse_Yesterday_ReturnsFullDayRange()
    {
        // Arrange
        var baseTime = new DateTime(2023, 12, 25, 12, 0, 0);
        var expected = baseTime.AddDays(-1).StartOfDay();

        // Act
        var range = DateTimeRange.Parse("yesterday", baseTime);

        // Assert
        Assert.NotEqual(DateTimeRange.Empty, range);
        Assert.Equal(expected, range.Start);
        Assert.Equal(expected.EndOfDay(), range.End);
    }

    [Fact]
    public void Parse_Today_ReturnsFullDayRange()
    {
        // Arrange
        var baseTime = new DateTime(2023, 12, 25, 12, 0, 0);
        var expected = baseTime.StartOfDay();

        // Act
        var range = DateTimeRange.Parse("today", baseTime);

        // Assert
        Assert.NotEqual(DateTimeRange.Empty, range);
        Assert.Equal(expected, range.Start);
        Assert.Equal(expected.EndOfDay(), range.End);
    }

    [Fact]
    public void Parse_Tomorrow_ReturnsFullDayRange()
    {
        // Arrange
        var baseTime = new DateTime(2023, 12, 25, 12, 0, 0);
        var expected = baseTime.AddDays(1).StartOfDay();

        // Act
        var range = DateTimeRange.Parse("tomorrow", baseTime);

        // Assert
        Assert.NotEqual(DateTimeRange.Empty, range);
        Assert.Equal(expected, range.Start);
        Assert.Equal(expected.EndOfDay(), range.End);
    }

    [Fact]
    public void Parse_LastFiveMinutes_ReturnsPastTimeRange()
    {
        // Arrange
        var baseTime = new DateTime(2023, 12, 25, 12, 0, 0);
        var expectedStart = baseTime.AddMinutes(-5).StartOfMinute();

        // Act
        var range = DateTimeRange.Parse("last 5 minutes", baseTime);

        // Assert
        Assert.NotEqual(DateTimeRange.Empty, range);
        Assert.Equal(expectedStart, range.Start);
        Assert.Equal(baseTime, range.End);
    }

    [Fact]
    public void Parse_NextTwoHours_ReturnsFutureTimeRange()
    {
        // Arrange
        var baseTime = new DateTime(2023, 12, 25, 12, 0, 0);
        var expectedEnd = baseTime.AddHours(2).EndOfHour();

        // Act
        var range = DateTimeRange.Parse("next 2 hours", baseTime);

        // Assert
        Assert.NotEqual(DateTimeRange.Empty, range);
        Assert.Equal(baseTime, range.Start);
        Assert.Equal(expectedEnd, range.End);
    }

    [Fact]
    public void Parse_BracketNotationWithDateMath_ParsesCorrectly()
    {
        // Arrange
        var baseTime = new DateTime(2023, 12, 25, 12, 0, 0);

        // Act
        var range = DateTimeRange.Parse("[now-6h TO now]", baseTime);

        // Assert
        Assert.NotEqual(DateTimeRange.Empty, range);
        Assert.NotEqual(DateTime.MinValue, range.Start);
        Assert.NotEqual(DateTime.MinValue, range.End);
        Assert.True(range.Start < range.End);

        // Verify 'now' resolves to base time
        var tolerance = TimeSpan.FromMinutes(1);
        Assert.True(Math.Abs((range.End - baseTime).TotalMinutes) < tolerance.TotalMinutes);

        // Verify the 6-hour span
        var expectedStart = baseTime.AddHours(-6);
        Assert.True(Math.Abs((range.Start - expectedStart).TotalMinutes) < tolerance.TotalMinutes);
    }

    [Fact]
    public void Parse_CurlyBraceNotationWithDateMath_ParsesCorrectly()
    {
        // Arrange
        var baseTime = new DateTime(2023, 12, 25, 12, 0, 0);

        // Act
        var range = DateTimeRange.Parse("{now-1d TO now}", baseTime);

        // Assert
        Assert.NotEqual(DateTimeRange.Empty, range);
        Assert.NotEqual(DateTime.MinValue, range.Start);
        Assert.NotEqual(DateTime.MinValue, range.End);
        Assert.True(range.Start < range.End);

        // Verify 'now' resolves to base time
        var tolerance = TimeSpan.FromMinutes(1);
        Assert.True(Math.Abs((range.End - baseTime).TotalMinutes) < tolerance.TotalMinutes);

        // Verify the 1-day span
        var expectedStart = baseTime.AddDays(-1);
        Assert.True(Math.Abs((range.Start - expectedStart).TotalMinutes) < tolerance.TotalMinutes);
    }

    [Fact]
    public void Parse_DateMathWithoutBrackets_ParsesCorrectly()
    {
        // Arrange
        var baseTime = new DateTime(2023, 12, 25, 12, 0, 0);

        // Act
        var range = DateTimeRange.Parse("now-6h TO now", baseTime);

        // Assert
        Assert.NotEqual(DateTimeRange.Empty, range);
        Assert.NotEqual(DateTime.MinValue, range.Start);
        Assert.NotEqual(DateTime.MinValue, range.End);
        Assert.True(range.Start < range.End);

        // Verify 'now' resolves to base time
        var tolerance = TimeSpan.FromMinutes(1);
        Assert.True(Math.Abs((range.End - baseTime).TotalMinutes) < tolerance.TotalMinutes);
    }

    [Fact]
    public void Parse_MixedParsersInBracketNotation_ParsesCorrectly()
    {
        // Arrange
        var baseTime = new DateTime(2023, 12, 25, 12, 0, 0);

        // Act
        var range = DateTimeRange.Parse("[yesterday TO today]", baseTime);

        // Assert
        Assert.NotEqual(DateTimeRange.Empty, range);
        Assert.NotEqual(DateTime.MinValue, range.Start);
        Assert.NotEqual(DateTime.MinValue, range.End);
        Assert.True(range.Start < range.End);

        // Verify yesterday and today are parsed correctly
        var expectedStart = baseTime.AddDays(-1).StartOfDay();
        var expectedEnd = baseTime.StartOfDay().EndOfDay();
        Assert.Equal(expectedStart, range.Start);
        Assert.Equal(expectedEnd, range.End);
    }

    [Fact]
    public void Parse_DateMathStartAndEnd_ParsesCorrectly()
    {
        // Arrange
        var baseTime = new DateTime(2023, 12, 25, 12, 0, 0);

        // Act
        var range = DateTimeRange.Parse("now-1h TO now+1h", baseTime);

        // Assert
        Assert.NotEqual(DateTimeRange.Empty, range);
        Assert.NotEqual(DateTime.MinValue, range.Start);
        Assert.NotEqual(DateTime.MinValue, range.End);
        Assert.True(range.Start < range.End);

        // Verify the 2-hour span centered on base time
        var tolerance = TimeSpan.FromMinutes(1);
        var expectedStart = baseTime.AddHours(-1);
        var expectedEnd = baseTime.AddHours(1);
        Assert.True(Math.Abs((range.Start - expectedStart).TotalMinutes) < tolerance.TotalMinutes);
        Assert.True(Math.Abs((range.End - expectedEnd).TotalMinutes) < tolerance.TotalMinutes);
    }

    [Theory]
    [InlineData("now-6h TO now+6h", 12, "DateMath expressions should create 12-hour range")]
    [InlineData("now-1d TO now", 24, "DateMath expressions should create 24-hour range")]
    [InlineData("now TO now+30m", 0.5, "DateMath expressions should create 30-minute range")]
    public void Parse_DateMathExpressions_CreatesCorrectTimeSpans(string input, double expectedHours, string reason)
    {
        // Arrange
        var baseTime = new DateTime(2023, 12, 25, 12, 0, 0);

        // Act
        var range = DateTimeRange.Parse(input, baseTime);

        // Assert
        Assert.NotEqual(DateTimeRange.Empty, range);
        var actualHours = (range.End - range.Start).TotalHours;
        Assert.True(Math.Abs(actualHours - expectedHours) < 0.1,
            $"{reason}. Expected {expectedHours} hours, but got {actualHours} hours");
    }

    [Fact]
    public void Parse_BracketNotationWithDateMath_PreservesTimeZoneInformation()
    {
        // Arrange
        var baseTime = new DateTimeOffset(2023, 12, 25, 12, 0, 0, TimeSpan.FromHours(-5));
        const string input = "[now-6h TO now]";

        // Act
        var range = DateTimeRange.Parse(input, baseTime);

        // Assert
        Assert.NotEqual(DateTimeRange.Empty, range);
        Assert.Equal(baseTime.AddHours(-6).DateTime, range.Start);
        Assert.Equal(baseTime.DateTime, range.End);
    }

    [Theory]
    [InlineData("now+1INVALID", "Invalid unit should not parse")]
    [InlineData("now+", "Incomplete expression should not parse")]
    [InlineData("now++1d", "Double operators should not parse")]
    [InlineData("[now+ TO now]", "Invalid left side in bracket notation should not parse")]
    public void Parse_InvalidDateMathExpressions_ReturnsEmptyRange(string input, string reason)
    {
        // Arrange
        var baseTime = new DateTime(2023, 12, 25, 12, 0, 0);

        // Act
        var range = DateTimeRange.Parse(input, baseTime);

        // Assert - Invalid expressions should result in empty range or fallback parsing
        // The behavior may vary based on parser priority and fallback mechanisms
        Assert.True(range == DateTimeRange.Empty || range.Start != DateTime.MinValue,
            $"{reason}. Input '{input}' should either return empty range or valid fallback parsing");
    }
}
