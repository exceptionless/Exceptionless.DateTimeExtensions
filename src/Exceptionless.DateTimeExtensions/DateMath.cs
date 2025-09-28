using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions;

/// <summary>
/// Provides Elasticsearch date math parsing functionality.
/// Supports: now, explicit dates with ||, operations (+, -, /), and time units (y, M, w, d, h, H, m, s).
/// Examples: now+1h, now-1d/d, 2001.02.01||+1M/d, 2025-01-01T01:25:35Z||+3d/d
///
/// Timezone Handling (following Elasticsearch standards):
/// - Explicit timezone (Z, +05:00, -08:00): Preserved from input
/// - No timezone: Uses current system timezone
///
/// References:
/// - https://www.elastic.co/guide/en/elasticsearch/reference/current/common-options.html#date-math
/// - https://www.elastic.co/guide/en/elasticsearch/reference/current/query-dsl-range-query.html#date-math-rounding
/// </summary>
public static class DateMath
{
    // Match date math expressions with positional and end anchors for flexible matching
    // Uses \G for positional matching and lookahead for boundary detection to support both
    // full string parsing and positional matching within TwoPartFormatParser
    internal static readonly Regex Parser = new(
        @"\G(?<anchor>now|(?<date>\d{4}-?\d{2}-?\d{2}(?:[T\s](?:\d{1,2}(?::?\d{2}(?::?\d{2})?)?(?:\.\d{1,3})?)?(?:[+-]\d{2}:?\d{2}|Z)?)?)\|\|)" +
        @"(?<operations>(?:[+\-/]\d*[yMwdhHms])*)(?=\s|$|[\]\}])",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Pre-compiled regex for operation parsing to avoid repeated compilation
    private static readonly Regex _operationRegex = new(@"([+\-/])(\d*)([yMwdhHms])", RegexOptions.Compiled);

    // Pre-compiled regex for offset parsing to avoid repeated compilation
    private static readonly Regex _offsetRegex = new(@"(Z|[+-]\d{2}:\d{2})$", RegexOptions.Compiled);

    /// <summary>
    /// Parses a date math expression and returns the resulting DateTimeOffset.
    /// </summary>
    /// <param name="expression">The date math expression to parse</param>
    /// <param name="relativeBaseTime">The base time to use for relative calculations (e.g., 'now')</param>
    /// <param name="isUpperLimit">Whether this is for an upper limit (affects rounding behavior)</param>
    /// <returns>The parsed DateTimeOffset</returns>
    /// <exception cref="ArgumentException">Thrown when the expression is invalid or cannot be parsed</exception>
    public static DateTimeOffset Parse(string expression, DateTimeOffset relativeBaseTime, bool isUpperLimit = false)
    {
        if (!TryParse(expression, relativeBaseTime, isUpperLimit, out DateTimeOffset result))
            throw new ArgumentException($"Invalid date math expression: {expression}", nameof(expression));

        return result;
    }

    /// <summary>
    /// Tries to parse a date math expression and returns the resulting DateTimeOffset.
    /// </summary>
    /// <param name="expression">The date math expression to parse</param>
    /// <param name="relativeBaseTime">The base time to use for relative calculations (e.g., 'now')</param>
    /// <param name="isUpperLimit">Whether this is for an upper limit (affects rounding behavior)</param>
    /// <param name="result">The parsed DateTimeOffset if successful</param>
    /// <returns>True if parsing succeeded, false otherwise</returns>
    public static bool TryParse(string expression, DateTimeOffset relativeBaseTime, bool isUpperLimit, out DateTimeOffset result)
    {
        result = default;

        if (String.IsNullOrEmpty(expression))
            return false;

        var match = Parser.Match(expression);
        if (!match.Success)
        {
            return TryParseFallbackDate(expression, relativeBaseTime.Offset, isUpperLimit, out result);
        }

        return TryParseFromMatch(match, relativeBaseTime, isUpperLimit, out result);
    }

    /// <summary>
    /// Parses a date math expression and returns the resulting DateTimeOffset using the specified timezone.
    /// </summary>
    /// <param name="expression">The date math expression to parse</param>
    /// <param name="timeZone">The timezone to use for 'now' calculations and dates without explicit timezone information</param>
    /// <param name="isUpperLimit">Whether this is for an upper limit (affects rounding behavior)</param>
    /// <returns>The parsed DateTimeOffset</returns>
    /// <exception cref="ArgumentException">Thrown when the expression is invalid or cannot be parsed</exception>
    /// <exception cref="ArgumentNullException">Thrown when timeZone is null</exception>
    public static DateTimeOffset Parse(string expression, TimeZoneInfo timeZone, bool isUpperLimit = false)
    {
        if (timeZone == null)
            throw new ArgumentNullException(nameof(timeZone));

        if (!TryParse(expression, timeZone, isUpperLimit, out DateTimeOffset result))
            throw new ArgumentException($"Invalid date math expression: {expression}", nameof(expression));

        return result;
    }

    /// <summary>
    /// Tries to parse a date math expression and returns the resulting DateTimeOffset using the specified timezone.
    /// </summary>
    /// <param name="expression">The date math expression to parse</param>
    /// <param name="timeZone">The timezone to use for 'now' calculations and dates without explicit timezone information</param>
    /// <param name="isUpperLimit">Whether this is for an upper limit (affects rounding behavior)</param>
    /// <param name="result">The parsed DateTimeOffset if successful</param>
    /// <returns>True if parsing succeeded, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when timeZone is null</exception>
    public static bool TryParse(string expression, TimeZoneInfo timeZone, bool isUpperLimit, out DateTimeOffset result)
    {
        if (timeZone == null)
            throw new ArgumentNullException(nameof(timeZone));

        result = default;

        if (String.IsNullOrEmpty(expression))
            return false;

        var match = Parser.Match(expression);
        if (!match.Success)
        {
            return TryParseFallbackDate(expression, timeZone, isUpperLimit, out result);
        }

        return TryParseFromMatch(match, timeZone, isUpperLimit, out result);
    }

    /// <summary>
    /// Tries to parse a date math expression from a regex match and returns the resulting DateTimeOffset.
    /// This method bypasses the regex matching for cases where the match is already available.
    /// </summary>
    /// <param name="match">The regex match containing the parsed expression groups</param>
    /// <param name="relativeBaseTime">The base time to use for relative calculations (e.g., 'now')</param>
    /// <param name="isUpperLimit">Whether this is for an upper limit (affects rounding behavior)</param>
    /// <param name="result">The parsed DateTimeOffset if successful</param>
    /// <returns>True if parsing succeeded, false otherwise</returns>
    public static bool TryParseFromMatch(Match match, DateTimeOffset relativeBaseTime, bool isUpperLimit, out DateTimeOffset result)
    {
        result = default;

        try
        {
            // Parse the anchor (now or explicit date)
            DateTimeOffset baseTime;
            string anchor = match.Groups["anchor"].Value;

            if (anchor.Equals("now", StringComparison.OrdinalIgnoreCase))
            {
                baseTime = relativeBaseTime;
            }
            else
            {
                // Parse explicit date from the date group
                string dateStr = match.Groups["date"].Value;
                if (!TryParseExplicitDate(dateStr, relativeBaseTime.Offset, out baseTime))
                    return false;
            }

            // Parse and apply operations
            string operations = match.Groups["operations"].Value;
            result = ApplyOperations(baseTime, operations, isUpperLimit);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Tries to parse a date math expression from a regex match and returns the resulting DateTimeOffset using the specified timezone.
    /// This method bypasses the regex matching for cases where the match is already available.
    /// </summary>
    /// <param name="match">The regex match containing the parsed expression groups</param>
    /// <param name="timeZone">The timezone to use for 'now' calculations and dates without explicit timezone information</param>
    /// <param name="isUpperLimit">Whether this is for an upper limit (affects rounding behavior)</param>
    /// <param name="result">The parsed DateTimeOffset if successful</param>
    /// <returns>True if parsing succeeded, false otherwise</returns>
    public static bool TryParseFromMatch(Match match, TimeZoneInfo timeZone, bool isUpperLimit, out DateTimeOffset result)
    {
        result = default;

        try
        {
            // Parse the anchor (now or explicit date)
            DateTimeOffset baseTime;
            string anchor = match.Groups["anchor"].Value;

            if (anchor.Equals("now", StringComparison.OrdinalIgnoreCase))
            {
                // Use current time in the specified timezone
                baseTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
            }
            else
            {
                // Parse explicit date from the date group
                string dateStr = match.Groups["date"].Value;
                TimeSpan offset = timeZone.GetUtcOffset(DateTime.UtcNow);
                if (!TryParseExplicitDate(dateStr, offset, out baseTime))
                    return false;
            }

            // Parse and apply operations
            string operations = match.Groups["operations"].Value;
            result = ApplyOperations(baseTime, operations, isUpperLimit);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the given expression is a valid date math expression.
    /// </summary>
    /// <param name="expression">The expression to validate</param>
    /// <returns>True if the expression is valid date math, false otherwise</returns>
    public static bool IsValidExpression(string expression)
    {
        if (String.IsNullOrEmpty(expression))
            return false;

        if (Parser.IsMatch(expression))
            return true;

        // Fallback: Check if it's a valid explicit date
        return TryParseFallbackDate(expression, TimeZoneInfo.Local, false, out _);
    }

    /// <summary>
    /// Attempts to parse the expression as an explicit date when date math parsing fails, using the provided timezone for missing offsets.
    /// </summary>
    /// <param name="expression">The original expression to interpret as an explicit date.</param>
    /// <param name="defaultTimeZone">The timezone applied when the expression lacks explicit offset information.</param>
    /// <param name="isUpperLimit">Whether the value should be treated as an upper bound, rounding end-of-day when applicable.</param>
    /// <param name="result">Receives the parsed <see cref="DateTimeOffset"/> when parsing succeeds.</param>
    /// <returns><see langword="true"/> when the expression is successfully parsed as an explicit date; otherwise, <see langword="false"/>.</returns>
    private static bool TryParseFallbackDate(string expression, TimeZoneInfo defaultTimeZone, bool isUpperLimit, out DateTimeOffset result)
    {
        if (_offsetRegex.IsMatch(expression) && DateTimeOffset.TryParse(expression, out DateTimeOffset explicitDate))
        {
            result = explicitDate;

            if (result.TimeOfDay == TimeSpan.Zero && isUpperLimit)
            {
                // If time is exactly midnight, and it's an upper limit, set to end of day
                result = result.EndOfDay();
            }

            return true;
        }

        if (DateTime.TryParse(expression, out DateTime dt))
        {
            result = new DateTimeOffset(dt, defaultTimeZone.GetUtcOffset(dt));

            if (result.TimeOfDay == TimeSpan.Zero && isUpperLimit)
            {
                // If time is exactly midnight, and it's an upper limit, set to end of day
                result = result.EndOfDay();
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// Attempts to parse the expression as an explicit date when date math parsing fails, using the provided offset for missing timezone information.
    /// </summary>
    /// <param name="expression">The original expression to interpret as an explicit date.</param>
    /// <param name="offset">The fallback UTC offset applied when the expression omits timezone data.</param>
    /// <param name="isUpperLimit">Whether the value should be treated as an upper bound, rounding to the end of day when appropriate.</param>
    /// <param name="result">Receives the parsed <see cref="DateTimeOffset"/> when parsing succeeds.</param>
    /// <returns><see langword="true"/> when the expression is successfully parsed as an explicit date; otherwise, <see langword="false"/>.</returns>
    private static bool TryParseFallbackDate(string expression, TimeSpan offset, bool isUpperLimit, out DateTimeOffset result)
    {
        if (_offsetRegex.IsMatch(expression) && DateTimeOffset.TryParse(expression, out DateTimeOffset explicitDate))
        {
            result = explicitDate;

            if (result.TimeOfDay == TimeSpan.Zero && isUpperLimit)
            {
                // If time is exactly midnight, and it's an upper limit, set to end of day
                result = result.EndOfDay();
            }

            return true;
        }

        if (DateTime.TryParse(expression, out DateTime dt))
        {
            result = new DateTimeOffset(dt, offset);

            if (result.TimeOfDay == TimeSpan.Zero && isUpperLimit)
            {
                // If time is exactly midnight, and it's an upper limit, set to end of day
                result = result.EndOfDay();
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// Attempts to parse an explicit date string with proper timezone handling.
    /// Supports various Elasticsearch-compatible date formats with optional timezone information.
    ///
    /// Performance-optimized with length checks and format ordering by likelihood.
    ///
    /// Timezone Behavior:
    /// - If timezone is specified (Z, +HH:MM, -HH:MM): Preserved from input
    /// - If no timezone specified: Uses the provided fallback offset
    ///
    /// This matches Elasticsearch's behavior where explicit timezone information takes precedence.
    /// </summary>
    /// <param name="dateStr">The date string to parse</param>
    /// <param name="offset">Fallback timezone offset for dates without explicit timezone</param>
    /// <param name="result">The parsed DateTimeOffset with correct timezone</param>
    /// <returns>True if parsing succeeded, false otherwise</returns>
    private static bool TryParseExplicitDate(string dateStr, TimeSpan offset, out DateTimeOffset result)
    {
        result = default;

        if (String.IsNullOrEmpty(dateStr))
            return false;

        int len = dateStr.Length;

        // Early exit for obviously invalid lengths
        if (len is < 4 or > 29) // Min: yyyy (4), Max: yyyy-MM-ddTHH:mm:ss.fffzzz (29)
            return false;

        // Fast character validation for year digits
        if (!Char.IsDigit(dateStr[0]) || !Char.IsDigit(dateStr[1]) ||
            !Char.IsDigit(dateStr[2]) || !Char.IsDigit(dateStr[3]))
            return false;

        // Detect timezone presence for smart format selection
        bool hasZ = dateStr[len - 1] == 'Z';
        bool hasTimezone = hasZ;
        if (!hasTimezone && len > 10) // Check for +/-HH:mm timezone format
        {
            for (int index = Math.Max(10, len - 6); index < len - 1; index++)
            {
                if (dateStr[index] is '+' or '-' && index + 1 < len && Char.IsDigit(dateStr[index + 1]))
                {
                    hasTimezone = true;
                    break;
                }
            }
        }

        // Length-based format selection for maximum performance
        // Only try formats that match the exact length to avoid unnecessary parsing attempts
        switch (len)
        {
            case 4: // Built-in: year (yyyy)
                return TryParseWithFormat(dateStr, "yyyy", offset, false, out result);

            case 7: // Built-in: year_month (yyyy-MM)
                if (dateStr[4] == '-')
                    return TryParseWithFormat(dateStr, "yyyy-MM", offset, false, out result);
                break;

            case 8: // Built-in: basic_date (yyyyMMdd)
                return TryParseWithFormat(dateStr, "yyyyMMdd", offset, false, out result);

            case 10: // Built-in: date (yyyy-MM-dd)
                if (dateStr[4] == '-' && dateStr[7] == '-')
                {
                    return TryParseWithFormat(dateStr, "yyyy-MM-dd", offset, false, out result);
                }
                break;

            case 13: // Built-in: date_hour (yyyy-MM-ddTHH)
                if (dateStr[4] == '-' && dateStr[7] == '-' && dateStr[10] == 'T')
                {
                    return TryParseWithFormat(dateStr, "yyyy-MM-ddTHH", offset, false, out result);
                }
                break;

            case 16: // Built-in: date_hour_minute (yyyy-MM-ddTHH:mm)
                if (dateStr[4] == '-' && dateStr[7] == '-' && dateStr[10] == 'T' && dateStr[13] == ':')
                {
                    return TryParseWithFormat(dateStr, "yyyy-MM-ddTHH:mm", offset, false, out result);
                }
                break;

            case 19: // Built-in: date_hour_minute_second (yyyy-MM-ddTHH:mm:ss)
                if (dateStr[4] == '-' && dateStr[7] == '-' && dateStr[10] == 'T' && dateStr[13] == ':' && dateStr[16] == ':')
                {
                    return TryParseWithFormat(dateStr, "yyyy-MM-ddTHH:mm:ss", offset, false, out result);
                }
                break;

            case 20: // Built-in: date_time_no_millis (yyyy-MM-ddTHH:mm:ssZ)
                if (hasZ && dateStr[4] == '-' && dateStr[7] == '-' && dateStr[10] == 'T' && dateStr[13] == ':' && dateStr[16] == ':')
                {
                    return TryParseWithFormat(dateStr, "yyyy-MM-ddTHH:mm:ssZ", offset, true, out result);
                }
                break;

            case 23: // Built-in: date_hour_minute_second_millis (yyyy-MM-ddTHH:mm:ss.fff)
                if (dateStr[4] == '-' && dateStr[7] == '-' && dateStr[10] == 'T' && dateStr[13] == ':' && dateStr[16] == ':' && dateStr[19] == '.')
                {
                    return TryParseWithFormat(dateStr, "yyyy-MM-ddTHH:mm:ss.fff", offset, false, out result);
                }
                break;

            case 24: // Built-in: date_time (yyyy-MM-ddTHH:mm:ss.fffZ)
                if (hasZ && dateStr[4] == '-' && dateStr[7] == '-' && dateStr[10] == 'T' && dateStr[13] == ':' && dateStr[16] == ':' && dateStr[19] == '.')
                {
                    return TryParseWithFormat(dateStr, "yyyy-MM-ddTHH:mm:ss.fffZ", offset, true, out result);
                }
                break;
        }

        // Handle RFC 822 timezone offset formats (variable lengths: +05:00, +0500, etc.)
        // Note: .NET uses 'zzz' pattern for timezone offsets like +05:00
        if (hasTimezone && !hasZ)
        {
            // Only try timezone formats for lengths that make sense
            if (len is >= 25 and <= 29) // +05:00 variants
            {
                if (dateStr.Contains(".")) // with milliseconds
                {
                    // yyyy-MM-ddTHH:mm:ss.fff+05:00
                    if (TryParseWithFormat(dateStr, "yyyy-MM-ddTHH:mm:ss.fffzzz", offset, true, out result))
                        return true;
                }
            }

            if (len is >= 22 and <= 25) // without milliseconds
            {
                // yyyy-MM-ddTHH:mm:ss+05:00
                if (TryParseWithFormat(dateStr, "yyyy-MM-ddTHH:mm:sszzz", offset, true, out result))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Helper method to parse with a specific format, handling timezone appropriately.
    /// </summary>
    private static bool TryParseWithFormat(string dateStr, string format, TimeSpan offset, bool hasTimezone, out DateTimeOffset result)
    {
        result = default;

        if (hasTimezone)
        {
            // Try parsing with timezone information preserved
            return DateTimeOffset.TryParseExact(dateStr, format, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out result);
        }

        // For formats without timezone, parse as DateTime and treat as if already in target timezone
        if (DateTime.TryParseExact(dateStr, format, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime dateTime))
        {
            // Treat the parsed DateTime as if it's already in the target timezone
            result = new DateTimeOffset(dateTime.Ticks, offset);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Applies date math operations to a base time.
    /// </summary>
    /// <param name="baseTime">The base time to apply operations to</param>
    /// <param name="operations">The operations string (e.g., "+1d-2h/d")</param>
    /// <param name="isUpperLimit">Whether this is for an upper limit (affects rounding)</param>
    /// <returns>The result after applying all operations</returns>
    public static DateTimeOffset ApplyOperations(DateTimeOffset baseTime, string operations, bool isUpperLimit = false)
    {
        if (String.IsNullOrEmpty(operations))
            return baseTime;

        var result = baseTime;
        var matches = _operationRegex.Matches(operations);

        // Validate that all operations were matched properly
        int totalMatchLength = matches.Cast<Match>().Sum(m => m.Length);
        if (totalMatchLength != operations.Length)
        {
            // If not all operations were matched, there are invalid operations
            throw new ArgumentException("Invalid operations");
        }

        // Validate that rounding operations (/) are only at the end
        // According to Elasticsearch spec, rounding must be the final operation
        bool foundRounding = false;
        for (int i = 0; i < matches.Count; i++)
        {
            string operation = matches[i].Groups[1].Value;
            if (operation == "/")
            {
                if (foundRounding)
                {
                    // Multiple rounding operations are not allowed
                    throw new ArgumentException("Multiple rounding operations are not allowed");
                }
                if (i != matches.Count - 1)
                {
                    // Rounding operation must be the last operation
                    throw new ArgumentException("Rounding operation must be the final operation");
                }
                foundRounding = true;
            }
        }

        foreach (Match opMatch in matches)
        {
            string operation = opMatch.Groups[1].Value;
            string amountStr = opMatch.Groups[2].Value;
            string unit = opMatch.Groups[3].Value;

            // Default amount is 1 if not specified
            int amount = String.IsNullOrEmpty(amountStr) ? 1 : Int32.Parse(amountStr);

            switch (operation)
            {
                case "+":
                    result = AddTimeUnit(result, amount, unit);
                    break;
                case "-":
                    result = AddTimeUnit(result, -amount, unit);
                    break;
                case "/":
                    result = RoundToUnit(result, unit, isUpperLimit);
                    break;
            }
        }

        return result;
    }

    /// <summary>
    /// Adds a time unit to a DateTimeOffset.
    /// </summary>
    /// <param name="dateTime">The base date time</param>
    /// <param name="amount">The amount to add</param>
    /// <param name="unit">The time unit (y, M, w, d, h, H, m, s)</param>
    /// <returns>The result after adding the time unit</returns>
    public static DateTimeOffset AddTimeUnit(DateTimeOffset dateTime, int amount, string unit)
    {
        try
        {
            return unit switch
            {
                "y" => dateTime.AddYears(amount),
                "M" => dateTime.AddMonths(amount), // Capital M for months
                "m" => dateTime.AddMinutes(amount), // Lowercase m for minutes
                "w" => dateTime.AddDays(amount * 7),
                "d" => dateTime.AddDays(amount), // Only lowercase d for days
                "h" or "H" => dateTime.AddHours(amount),
                "s" => dateTime.AddSeconds(amount),
                _ => throw new ArgumentException($"Invalid time unit: {unit}")
            };
        }
        catch (ArgumentOutOfRangeException)
        {
            // Return original date if operation would overflow
            return dateTime;
        }
    }

    /// <summary>
    /// Rounds a DateTimeOffset to a time unit.
    /// </summary>
    /// <param name="dateTime">The date time to round</param>
    /// <param name="unit">The time unit to round to (y, M, w, d, h, H, m, s)</param>
    /// <param name="isUpperLimit">Whether to round up (end of period) or down (start of period)</param>
    /// <returns>The rounded DateTimeOffset</returns>
    public static DateTimeOffset RoundToUnit(DateTimeOffset dateTime, string unit, bool isUpperLimit = false)
    {
        return unit switch
        {
            "y" => isUpperLimit ? dateTime.EndOfYear() : dateTime.StartOfYear(),
            "M" => isUpperLimit ? dateTime.EndOfMonth() : dateTime.StartOfMonth(),
            "w" => isUpperLimit ? dateTime.EndOfWeek() : dateTime.StartOfWeek(),
            "d" => isUpperLimit ? dateTime.EndOfDay() : dateTime.StartOfDay(), // Only lowercase d for days
            "h" or "H" => isUpperLimit ? dateTime.EndOfHour() : dateTime.StartOfHour(),
            "m" => isUpperLimit ? dateTime.EndOfMinute() : dateTime.StartOfMinute(),
            "s" => isUpperLimit ? dateTime.EndOfSecond() : dateTime.StartOfSecond(),
            _ => throw new ArgumentException($"Invalid time unit for rounding: {unit}")
        };
    }
}
