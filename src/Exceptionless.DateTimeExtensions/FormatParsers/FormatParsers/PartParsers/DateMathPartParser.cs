using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;

/// <summary>
/// Parses Elasticsearch date math expressions with proper timezone support.
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
[Priority(35)]
public class DateMathPartParser : IPartParser
{
    // Match date math expressions with anchors and operations
    private static readonly Regex _parser = new(
        @"^(?<anchor>now|(?<date>\d{4}[-.]?\d{2}[-.]?\d{2}(?:[T\s](?:\d{1,2}(?::?\d{2}(?::?\d{2})?)?(?:\.\d{1,3})?)?(?:[+-]\d{2}:?\d{2}|Z)?)?)\|\|)" +
        @"(?<operations>(?:[+\-/]\d*[yMwdhHms])*)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Pre-compiled regex for operation parsing to avoid repeated compilation
    private static readonly Regex _operationRegex = new(@"([+\-/])(\d*)([yMwdhHms])", RegexOptions.Compiled);

    public Regex Regex => _parser;

    public DateTimeOffset? Parse(Match match, DateTimeOffset relativeBaseTime, bool isUpperLimit)
    {
        if (!match.Success)
            return null;

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
                    return null;
            }

            // Parse and apply operations
            string operations = match.Groups["operations"].Value;
            return ApplyOperations(baseTime, operations, isUpperLimit);
        }
        catch
        {
            return null;
        }
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
                    return TryParseWithFormat(dateStr, "yyyy-MM-dd", offset, false, out result);
                break;

            case 13: // Built-in: date_hour (yyyy-MM-ddTHH)
                if (dateStr[4] == '-' && dateStr[7] == '-' && dateStr[10] == 'T')
                    return TryParseWithFormat(dateStr, "yyyy-MM-ddTHH", offset, false, out result);
                break;

            case 16: // Built-in: date_hour_minute (yyyy-MM-ddTHH:mm)
                if (dateStr[4] == '-' && dateStr[7] == '-' && dateStr[10] == 'T' && dateStr[13] == ':')
                    return TryParseWithFormat(dateStr, "yyyy-MM-ddTHH:mm", offset, false, out result);
                break;

            case 19: // Built-in: date_hour_minute_second (yyyy-MM-ddTHH:mm:ss)
                if (dateStr[4] == '-' && dateStr[7] == '-' && dateStr[10] == 'T' && dateStr[13] == ':' && dateStr[16] == ':')
                    return TryParseWithFormat(dateStr, "yyyy-MM-ddTHH:mm:ss", offset, false, out result);
                break;

            case 20: // Built-in: date_time_no_millis (yyyy-MM-ddTHH:mm:ssZ)
                if (hasZ && dateStr[4] == '-' && dateStr[7] == '-' && dateStr[10] == 'T' && dateStr[13] == ':' && dateStr[16] == ':')
                    return TryParseWithFormat(dateStr, "yyyy-MM-ddTHH:mm:ssZ", offset, true, out result);
                break;

            case 23: // Built-in: date_hour_minute_second_millis (yyyy-MM-ddTHH:mm:ss.fff)
                if (dateStr[4] == '-' && dateStr[7] == '-' && dateStr[10] == 'T' && dateStr[13] == ':' && dateStr[16] == ':' && dateStr[19] == '.')
                    return TryParseWithFormat(dateStr, "yyyy-MM-ddTHH:mm:ss.fff", offset, false, out result);
                break;

            case 24: // Built-in: date_time (yyyy-MM-ddTHH:mm:ss.fffZ)
                if (hasZ && dateStr[4] == '-' && dateStr[7] == '-' && dateStr[10] == 'T' && dateStr[13] == ':' && dateStr[16] == ':' && dateStr[19] == '.')
                    return TryParseWithFormat(dateStr, "yyyy-MM-ddTHH:mm:ss.fffZ", offset, true, out result);
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
                    // Try: yyyy-MM-ddTHH:mm:ss.fff+05:00
                    if (TryParseWithFormat(dateStr, "yyyy-MM-ddTHH:mm:ss.fffzzz", offset, true, out result))
                        return true;
                }
            }

            if (len is >= 22 and <= 25) // without milliseconds
            {
                // Try: yyyy-MM-ddTHH:mm:ss+05:00
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

    private static DateTimeOffset ApplyOperations(DateTimeOffset baseTime, string operations, bool isUpperLimit)
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

    private static DateTimeOffset AddTimeUnit(DateTimeOffset dateTime, int amount, string unit)
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

    private static DateTimeOffset RoundToUnit(DateTimeOffset dateTime, string unit, bool isUpperLimit)
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
