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
    // Match date math expressions with anchors and operations
    internal static readonly Regex Parser = new(
        @"^(?<anchor>now|(?<date>\d{4}[-.]?\d{2}[-.]?\d{2}(?:[T\s](?:\d{1,2}(?::?\d{2}(?::?\d{2})?)?(?:\.\d{1,3})?)?(?:[+-]\d{2}:?\d{2}|Z)?)?)\|\|)" +
        @"(?<operations>(?:[+\-/]\d*[yMwdhHms])*)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Pre-compiled regex for operation parsing to avoid repeated compilation
    private static readonly Regex _operationRegex = new(@"([+\-/])(\d*)([yMwdhHms])", RegexOptions.Compiled);

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
            return false;

        return TryParseFromMatch(match, relativeBaseTime, isUpperLimit, out result);
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
    /// Checks if the given expression is a valid date math expression.
    /// </summary>
    /// <param name="expression">The expression to validate</param>
    /// <returns>True if the expression is valid date math, false otherwise</returns>
    public static bool IsValidExpression(string expression)
    {
        if (String.IsNullOrEmpty(expression))
            return false;

        return Parser.IsMatch(expression);
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

            case 7: // Built-in: year_month (yyyy-MM or yyyy.MM)
                if (dateStr[4] is '-' or '.')
                    return TryParseWithFormat(dateStr, dateStr[4] == '-' ? "yyyy-MM" : "yyyy.MM", offset, false, out result);
                break;

            case 8: // Built-in: basic_date (yyyyMMdd)
                return TryParseWithFormat(dateStr, "yyyyMMdd", offset, false, out result);

            case 10: // Built-in: date (yyyy-MM-dd or yyyy.MM.dd)
                if (dateStr[4] is '-' or '.' && dateStr[7] is '-' or '.')
                {
                    string format = dateStr[4] == '-' ? "yyyy-MM-dd" : "yyyy.MM.dd";
                    return TryParseWithFormat(dateStr, format, offset, false, out result);
                }
                break;

            case 13: // Built-in: date_hour (yyyy-MM-ddTHH or yyyy.MM.ddTHH)
                if (dateStr[4] is '-' or '.' && dateStr[7] is '-' or '.' && dateStr[10] == 'T')
                {
                    string format = dateStr[4] == '-' ? "yyyy-MM-ddTHH" : "yyyy.MM.ddTHH";
                    return TryParseWithFormat(dateStr, format, offset, false, out result);
                }
                break;

            case 16: // Built-in: date_hour_minute (yyyy-MM-ddTHH:mm or yyyy.MM.ddTHH:mm)
                if (dateStr[4] is '-' or '.' && dateStr[7] is '-' or '.' && dateStr[10] == 'T' && dateStr[13] == ':')
                {
                    string format = dateStr[4] == '-' ? "yyyy-MM-ddTHH:mm" : "yyyy.MM.ddTHH:mm";
                    return TryParseWithFormat(dateStr, format, offset, false, out result);
                }
                break;

            case 19: // Built-in: date_hour_minute_second (yyyy-MM-ddTHH:mm:ss or yyyy.MM.ddTHH:mm:ss)
                if (dateStr[4] is '-' or '.' && dateStr[7] is '-' or '.' && dateStr[10] == 'T' && dateStr[13] == ':' && dateStr[16] == ':')
                {
                    string format = dateStr[4] == '-' ? "yyyy-MM-ddTHH:mm:ss" : "yyyy.MM.ddTHH:mm:ss";
                    return TryParseWithFormat(dateStr, format, offset, false, out result);
                }
                break;

            case 20: // Built-in: date_time_no_millis (yyyy-MM-ddTHH:mm:ssZ or yyyy.MM.ddTHH:mm:ssZ)
                if (hasZ && dateStr[4] is '-' or '.' && dateStr[7] is '-' or '.' && dateStr[10] == 'T' && dateStr[13] == ':' && dateStr[16] == ':')
                {
                    string format = dateStr[4] == '-' ? "yyyy-MM-ddTHH:mm:ssZ" : "yyyy.MM.ddTHH:mm:ssZ";
                    return TryParseWithFormat(dateStr, format, offset, true, out result);
                }
                break;

            case 23: // Built-in: date_hour_minute_second_millis (yyyy-MM-ddTHH:mm:ss.fff or yyyy.MM.ddTHH:mm:ss.fff)
                if (dateStr[4] is '-' or '.' && dateStr[7] is '-' or '.' && dateStr[10] == 'T' && dateStr[13] == ':' && dateStr[16] == ':' && dateStr[19] == '.')
                {
                    string format = dateStr[4] == '-' ? "yyyy-MM-ddTHH:mm:ss.fff" : "yyyy.MM.ddTHH:mm:ss.fff";
                    return TryParseWithFormat(dateStr, format, offset, false, out result);
                }
                break;

            case 24: // Built-in: date_time (yyyy-MM-ddTHH:mm:ss.fffZ or yyyy.MM.ddTHH:mm:ss.fffZ)
                if (hasZ && dateStr[4] is '-' or '.' && dateStr[7] is '-' or '.' && dateStr[10] == 'T' && dateStr[13] == ':' && dateStr[16] == ':' && dateStr[19] == '.')
                {
                    string format = dateStr[4] == '-' ? "yyyy-MM-ddTHH:mm:ss.fffZ" : "yyyy.MM.ddTHH:mm:ss.fffZ";
                    return TryParseWithFormat(dateStr, format, offset, true, out result);
                }
                break;
        }

        // Handle RFC 822 timezone offset formats (variable lengths: +05:00, +0500, etc.)
        // Note: .NET uses 'zzz' pattern for timezone offsets like +05:00
        if (hasTimezone && !hasZ)
        {
            // Determine the date separator for format construction
            char dateSeparator = (len > 4 && dateStr[4] == '.') ? '.' : '-';

            // Only try timezone formats for lengths that make sense
            if (len is >= 25 and <= 29) // +05:00 variants
            {
                if (dateStr.Contains(".")) // with milliseconds
                {
                    // Try both separators: yyyy-MM-ddTHH:mm:ss.fff+05:00 or yyyy.MM.ddTHH:mm:ss.fff+05:00
                    string format = dateSeparator == '.'
                        ? "yyyy.MM.ddTHH:mm:ss.fffzzz"
                        : "yyyy-MM-ddTHH:mm:ss.fffzzz";
                    if (TryParseWithFormat(dateStr, format, offset, true, out result))
                        return true;
                }
            }

            if (len is >= 22 and <= 25) // without milliseconds
            {
                // Try both separators: yyyy-MM-ddTHH:mm:ss+05:00 or yyyy.MM.ddTHH:mm:ss+05:00
                string format = dateSeparator == '.'
                    ? "yyyy.MM.ddTHH:mm:sszzz"
                    : "yyyy-MM-ddTHH:mm:sszzz";
                if (TryParseWithFormat(dateStr, format, offset, true, out result))
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
