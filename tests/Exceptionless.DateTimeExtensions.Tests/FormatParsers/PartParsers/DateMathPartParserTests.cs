using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;
using Xunit;
using Xunit.Abstractions;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers.PartParsers;

public class DateMathPartParserTests : PartParserTestsBase
{
    public DateMathPartParserTests(ITestOutputHelper output) : base(output) { }

    [Theory]
    [MemberData(nameof(NowInputs))]
    public void ParseNowInput(string input, bool isUpperLimit, DateTimeOffset? expected)
    {
        ValidateInput(new DateMathPartParser(), input, isUpperLimit, expected);
    }

    [Theory]
    [MemberData(nameof(ExplicitDateInputs))]
    public void ParseExplicitDateInput(string input, bool isUpperLimit, DateTimeOffset? expected)
    {
        ValidateInput(new DateMathPartParser(), input, isUpperLimit, expected);
    }

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public void ParseInvalidInput(string input, bool isUpperLimit, DateTimeOffset? expected)
    {
        ValidateInput(new DateMathPartParser(), input, isUpperLimit, expected);
    }

    [Theory]
    [MemberData(nameof(ComplexOperationInputs))]
    public void ParseComplexOperations(string input, bool isUpperLimit, DateTimeOffset? expected)
    {
        ValidateInput(new DateMathPartParser(), input, isUpperLimit, expected);
    }

    [Theory]
    [MemberData(nameof(RoundingInputs))]
    public void ParseRoundingOperations(string input, bool isUpperLimit, DateTimeOffset? expected)
    {
        ValidateInput(new DateMathPartParser(), input, isUpperLimit, expected);
    }

    [Theory]
    [MemberData(nameof(EdgeCaseInputs))]
    public void ParseEdgeCases(string input, bool isUpperLimit, DateTimeOffset? expected)
    {
        ValidateInput(new DateMathPartParser(), input, isUpperLimit, expected);
    }

    public static IEnumerable<object[]> NowInputs
    {
        get
        {
            return
            [
                // Basic "now" anchor
                ["now", false, _now],
                ["now", true, _now],

                // Now with single operations
                ["now+1h", false, _now.AddHours(1)],
                ["now+1h", true, _now.AddHours(1)],
                ["now-1h", false, _now.AddHours(-1)],
                ["now-1h", true, _now.AddHours(-1)],
                ["now+1d", false, _now.AddDays(1)],
                ["now+1d", true, _now.AddDays(1)],
                ["now-1d", false, _now.AddDays(-1)],
                ["now-1d", true, _now.AddDays(-1)],
                ["now+1w", false, _now.AddDays(7)],
                ["now+1w", true, _now.AddDays(7)],
                ["now-1w", false, _now.AddDays(-7)],
                ["now-1w", true, _now.AddDays(-7)],
                ["now+1M", false, _now.AddMonths(1)],
                ["now+1M", true, _now.AddMonths(1)],
                ["now-1M", false, _now.AddMonths(-1)],
                ["now-1M", true, _now.AddMonths(-1)],
                ["now+1y", false, _now.AddYears(1)],
                ["now+1y", true, _now.AddYears(1)],
                ["now-1y", false, _now.AddYears(-1)],
                ["now-1y", true, _now.AddYears(-1)],
                ["now+1m", false, _now.AddMinutes(1)],
                ["now+1m", true, _now.AddMinutes(1)],
                ["now-1m", false, _now.AddMinutes(-1)],
                ["now-1m", true, _now.AddMinutes(-1)],
                ["now+1s", false, _now.AddSeconds(1)],
                ["now+1s", true, _now.AddSeconds(1)],
                ["now-1s", false, _now.AddSeconds(-1)],
                ["now-1s", true, _now.AddSeconds(-1)],

                // Multi-digit amounts
                ["now+10h", false, _now.AddHours(10)],
                ["now+24h", true, _now.AddHours(24)],
                ["now-365d", false, _now.AddDays(-365)],
                ["now+100y", true, _now.AddYears(100)],

                // Capital H for hours
                ["now+1H", false, _now.AddHours(1)],
                ["now-2H", true, _now.AddHours(-2)],

                // Without amount (defaults to 1)
                ["now+h", false, _now.AddHours(1)],
                ["now-d", true, _now.AddDays(-1)],
                ["now+M", false, _now.AddMonths(1)],
                ["now-y", true, _now.AddYears(-1)]
            ];
        }
    }

    public static IEnumerable<object[]> ExplicitDateInputs
    {
        get
        {
            var baseDate = new DateTimeOffset(2001, 2, 1, 0, 0, 0, _now.Offset);

            return
            [
                // Basic explicit date formats (officially supported by Elasticsearch)
                ["2001-02-01||", false, baseDate],
                ["2001-02-01||", true, baseDate],
                ["20010201||", false, baseDate],
                ["20010201||", true, baseDate],

                // With time components (ISO 8601 formats)
                ["2001-02-01T12:30:45||", false, new DateTimeOffset(2001, 2, 1, 12, 30, 45, _now.Offset)],
                ["2001-02-01T12:30:45||", true, new DateTimeOffset(2001, 2, 1, 12, 30, 45, _now.Offset)],
                ["2001-02-01T12:30||", false, new DateTimeOffset(2001, 2, 1, 12, 30, 0, _now.Offset)],
                ["2001-02-01T12:30||", true, new DateTimeOffset(2001, 2, 1, 12, 30, 0, _now.Offset)],
                ["2001-02-01T12||", false, new DateTimeOffset(2001, 2, 1, 12, 0, 0, _now.Offset)],
                ["2001-02-01T12||", true, new DateTimeOffset(2001, 2, 1, 12, 0, 0, _now.Offset)],

                // With operations
                ["2001-02-01||+1M", false, baseDate.AddMonths(1)],
                ["2001-02-01||+1M", true, baseDate.AddMonths(1)],
                ["2001-02-01||+1y", false, baseDate.AddYears(1)],
                ["2001-02-01||+1y", true, baseDate.AddYears(1)],
                ["2001-02-01||-1d", false, baseDate.AddDays(-1)],
                ["2001-02-01||-1d", true, baseDate.AddDays(-1)],

                // With operations and rounding (basic_date format + operations)
                ["20010201||+1M/d", false, baseDate.AddMonths(1).StartOfDay()],
                ["20010201||+1M/d", true, baseDate.AddMonths(1).EndOfDay()],

                // User's specific test case - UTC date with operations and rounding
                ["2025-01-01T01:25:35Z||+3d/d", false, new DateTimeOffset(2025, 1, 4, 0, 0, 0, TimeSpan.Zero)],
                ["2025-01-01T01:25:35Z||+3d/d", true, new DateTimeOffset(2025, 1, 4, 23, 59, 59, 999, TimeSpan.Zero)],

                // Timezone variations - should preserve the timezone
                ["2023-06-15T14:30:00Z||", false, new DateTimeOffset(2023, 6, 15, 14, 30, 0, TimeSpan.Zero)],
                ["2023-06-15T14:30:00Z||", true, new DateTimeOffset(2023, 6, 15, 14, 30, 0, TimeSpan.Zero)],
                ["2023-06-15T14:30:00+05:00||", false, new DateTimeOffset(2023, 6, 15, 14, 30, 0, TimeSpan.FromHours(5))],
                ["2023-06-15T14:30:00+05:00||", true, new DateTimeOffset(2023, 6, 15, 14, 30, 0, TimeSpan.FromHours(5))],
                ["2023-06-15T14:30:00-08:00||", false, new DateTimeOffset(2023, 6, 15, 14, 30, 0, TimeSpan.FromHours(-8))],
                ["2023-06-15T14:30:00-08:00||", true, new DateTimeOffset(2023, 6, 15, 14, 30, 0, TimeSpan.FromHours(-8))],

                // Dates without timezone - should use current timezone
                ["2023-06-15T14:30:00||", false, new DateTimeOffset(2023, 6, 15, 14, 30, 0, _now.Offset)],
                ["2023-06-15T14:30:00||", true, new DateTimeOffset(2023, 6, 15, 14, 30, 0, _now.Offset)],

                // Milliseconds support with and without timezone
                ["2023-01-01T12:00:00.123||", false, new DateTimeOffset(2023, 1, 1, 12, 0, 0, 123, _now.Offset)],
                ["2023-01-01T12:00:00.123||", true, new DateTimeOffset(2023, 1, 1, 12, 0, 0, 123, _now.Offset)],
                ["2023-01-01T12:00:00.123Z||", false, new DateTimeOffset(2023, 1, 1, 12, 0, 0, 123, TimeSpan.Zero)],
                ["2023-01-01T12:00:00.123Z||", true, new DateTimeOffset(2023, 1, 1, 12, 0, 0, 123, TimeSpan.Zero)],
                ["2023-01-01T12:00:00.123+02:00||", false, new DateTimeOffset(2023, 1, 1, 12, 0, 0, 123, TimeSpan.FromHours(2))],
                ["2023-01-01T12:00:00.123+02:00||", true, new DateTimeOffset(2023, 1, 1, 12, 0, 0, 123, TimeSpan.FromHours(2))],

                // Basic format variations (yyyyMMdd is officially supported)
                ["20230615||", false, new DateTimeOffset(2023, 6, 15, 0, 0, 0, _now.Offset)],
                ["20230615||", true, new DateTimeOffset(2023, 6, 15, 0, 0, 0, _now.Offset)]
            ];
        }
    }

    public static IEnumerable<object[]> ComplexOperationInputs
    {
        get
        {
            return
            [
                // Multiple operations
                ["now+1d+1h", false, _now.AddDays(1).AddHours(1)],
                ["now+1d+1h", true, _now.AddDays(1).AddHours(1)],
                ["now-1d-1h", false, _now.AddDays(-1).AddHours(-1)],
                ["now-1d-1h", true, _now.AddDays(-1).AddHours(-1)],
                ["now+1M-1d", false, _now.AddMonths(1).AddDays(-1)],
                ["now+1M-1d", true, _now.AddMonths(1).AddDays(-1)],
                ["now+1y+6M+15d", false, _now.AddYears(1).AddMonths(6).AddDays(15)],
                ["now+1y+6M+15d", true, _now.AddYears(1).AddMonths(6).AddDays(15)],

                // Mixed units and operations
                ["now+2h+30m+45s", false, _now.AddHours(2).AddMinutes(30).AddSeconds(45)],
                ["now+2h+30m+45s", true, _now.AddHours(2).AddMinutes(30).AddSeconds(45)],
                ["now-1w+2d", false, _now.AddDays(-7).AddDays(2)],
                ["now-1w+2d", true, _now.AddDays(-7).AddDays(2)],

                // Operations with explicit dates and timezones
                ["2025-01-01T01:25:35||+3d/d", false, new DateTimeOffset(2025, 1, 4, 0, 0, 0, _now.Offset)],
                ["2025-01-01T01:25:35||+3d/d", true, new DateTimeOffset(2025, 1, 4, 23, 59, 59, 999, _now.Offset)],

                // Complex operations with UTC timezone
                ["2025-01-01T01:25:35Z||+3d/d", false, new DateTimeOffset(2025, 1, 4, 0, 0, 0, TimeSpan.Zero)],
                ["2025-01-01T01:25:35Z||+3d/d", true, new DateTimeOffset(2025, 1, 4, 23, 59, 59, 999, TimeSpan.Zero)],
                ["2023-12-31T23:59:59Z||+1d+1h", false, new DateTimeOffset(2024, 1, 2, 0, 59, 59, TimeSpan.Zero)],
                ["2023-12-31T23:59:59Z||+1d+1h", true, new DateTimeOffset(2024, 1, 2, 0, 59, 59, TimeSpan.Zero)],

                // Complex operations with timezone offsets
                ["2023-06-15T10:00:00+05:00||+1d-2h", false, new DateTimeOffset(2023, 6, 16, 8, 0, 0, TimeSpan.FromHours(5))],
                ["2023-06-15T10:00:00+05:00||+1d-2h", true, new DateTimeOffset(2023, 6, 16, 8, 0, 0, TimeSpan.FromHours(5))]
            ];
        }
    }

    public static IEnumerable<object[]> RoundingInputs
    {
        get
        {
            return
            [
                // Rounding to different units
                ["now/d", false, _now.StartOfDay()],
                ["now/d", true, _now.EndOfDay()],
                ["now/h", false, _now.StartOfHour()],
                ["now/h", true, _now.EndOfHour()],
                ["now/H", false, _now.StartOfHour()],
                ["now/H", true, _now.EndOfHour()],
                ["now/m", false, _now.StartOfMinute()],
                ["now/m", true, _now.EndOfMinute()],
                ["now/s", false, _now.StartOfSecond()],
                ["now/s", true, _now.EndOfSecond()],
                ["now/w", false, _now.StartOfWeek()],
                ["now/w", true, _now.EndOfWeek()],
                ["now/M", false, _now.StartOfMonth()],
                ["now/M", true, _now.EndOfMonth()],
                ["now/y", false, _now.StartOfYear()],
                ["now/y", true, _now.EndOfYear()],

                // Combined operations with rounding (Elasticsearch example)
                ["now-1h/d", false, _now.AddHours(-1).StartOfDay()],
                ["now-1h/d", true, _now.AddHours(-1).EndOfDay()],
                ["now+1d/h", false, _now.AddDays(1).StartOfHour()],
                ["now+1d/h", true, _now.AddDays(1).EndOfHour()],

                // Multiple operations ending with rounding
                ["now+1M-1d/d", false, _now.AddMonths(1).AddDays(-1).StartOfDay()],
                ["now+1M-1d/d", true, _now.AddMonths(1).AddDays(-1).EndOfDay()]
            ];
        }
    }

    public static IEnumerable<object[]> EdgeCaseInputs
    {
        get
        {
            return
            [
                // Case sensitivity
                ["NOW", false, _now],
                ["NOW", true, _now],
                ["Now", false, _now],
                ["Now", true, _now],

                // Zero amounts (edge case, should work)
                ["now+0d", false, _now],
                ["now+0d", true, _now],
                ["now-0h", false, _now],
                ["now-0h", true, _now],

                // Large amounts (but not overflow)
                ["now+100y", false, _now.AddYears(100)],
                ["now+100y", true, _now.AddYears(100)],

                // Overflow case - should return original date
                ["now+9999y", false, _now], // AddYears would overflow, should return original
                ["now+9999y", true, _now],

                // Mixed case units
                ["now+1D", false, null], // Should fail - lowercase d required
                ["now+1D", true, null],
                ["now+1HOUR", false, null], // Should fail - single char required
                ["now+1HOUR", true, null]
            ];
        }
    }

    public static IEnumerable<object[]> InvalidInputs
    {
        get
        {
            return
            [
                // Invalid formats
                ["invalid", false, null],
                ["invalid", true, null],
                ["blah", false, null],
                ["blah blah", true, null],
                ["", false, null],
                ["", true, null],

                // Invalid date formats
                ["2001-13-01||", false, null], // Invalid month
                ["2001-13-01||", true, null],
                ["2001-02-30||", false, null], // Invalid day for February
                ["2001-02-30||", true, null],
                ["invalid-date||", false, null],
                ["invalid-date||", true, null],

                // Invalid operations
                ["now+", false, null], // Missing unit
                ["now+", true, null],
                ["now+1", false, null], // Missing unit
                ["now+1", true, null],
                ["now+1x", false, null], // Invalid unit
                ["now+1x", true, null],
                ["now++1d", false, null], // Double operator
                ["now++1d", true, null],
                ["now+1d+", false, null], // Trailing operator
                ["now+1d+", true, null],

                // Invalid anchor
                ["yesterday+1d", false, null], // Invalid anchor
                ["yesterday+1d", true, null],
                ["2001||+1d", false, null], // Incomplete date
                ["2001||+1d", true, null],

                // Invalid timezone formats
                // Invalid date components
                ["2023-00-01||", false, null], // Invalid month
                ["2023-00-01||", true, null],
                ["2023-01-32||", false, null], // Invalid day
                ["2023-01-32||", true, null],
                ["2023-02-29||", false, null], // Invalid day for non-leap year
                ["2023-02-29||", true, null]
            ];
        }
    }
}
