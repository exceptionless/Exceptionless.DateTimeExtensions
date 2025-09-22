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
            var result = ApplyOperations(baseTime, operations, isUpperLimit);

            return result;
        }
        catch
        {
            // Return null for any parsing errors to maintain robustness
            return null;
        }
    }

    /// <summary>
    /// Attempts to parse an explicit date string with proper timezone handling.
    /// Supports various Elasticsearch-compatible date formats with optional timezone information.
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

        if (string.IsNullOrEmpty(dateStr))
            return false;

        // Try various formats that Elasticsearch supports
        string[] formats = {
            "yyyy-MM-dd",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm",
            "yyyy-MM-ddTHH",
            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy-MM-ddTHH:mm:ss.fff",
            "yyyy-MM-ddTHH:mm:ss.fffZ",
            "yyyy-MM-ddTHH:mm:sszzz",
            "yyyy-MM-ddTHH:mm:ss.fffzzz",
            "yyyy.MM.dd",
            "yyyy.MM.ddTHH:mm:ss",
            "yyyy.MM.ddTHH:mm",
            "yyyy.MM.ddTHH",
            "yyyy.MM.ddTHH:mm:ssZ",
            "yyyy.MM.ddTHH:mm:ss.fff",
            "yyyy.MM.ddTHH:mm:ss.fffZ",
            "yyyy.MM.ddTHH:mm:sszzz",
            "yyyy.MM.ddTHH:mm:ss.fffzzz",
            "yyyyMMdd",
            "yyyyMMddTHHmmss",
            "yyyyMMddTHHmm",
            "yyyyMMddTHH",
            "yyyyMMddTHHmmssZ",
            "yyyyMMddTHHmmss.fff",
            "yyyyMMddTHHmmss.fffZ",
            "yyyyMMddTHHmmsszzz",
            "yyyyMMddTHHmmss.fffzzz"
        };

        foreach (string format in formats)
        {
            // Handle timezone-aware formats differently from timezone-naive formats
            if (format.EndsWith("Z") || format.Contains("zzz"))
            {
                // Try parsing with timezone information preserved
                if (DateTimeOffset.TryParseExact(dateStr, format, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out result))
                {
                    return true;
                }
            }
            else
            {
                // For formats without timezone, parse as DateTime and treat as if already in target timezone
                if (DateTime.TryParseExact(dateStr, format, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime dateTime))
                {
                    // Treat the parsed DateTime as if it's already in the target timezone
                    // This avoids any conversion issues
                    result = new DateTimeOffset(dateTime.Ticks, offset);
                    return true;
                }
            }
        }

        return false;
    }

    private static DateTimeOffset ApplyOperations(DateTimeOffset baseTime, string operations, bool isUpperLimit)
    {
        if (string.IsNullOrEmpty(operations))
            return baseTime;

        var result = baseTime;
        var operationRegex = new Regex(@"([+\-/])(\d*)([yMwdhHms])", RegexOptions.Compiled);
        var matches = operationRegex.Matches(operations);

        // Validate that all operations were matched properly
        var totalMatchLength = matches.Cast<Match>().Sum(m => m.Length);
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
            int amount = string.IsNullOrEmpty(amountStr) ? 1 : int.Parse(amountStr);

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
                "d" => dateTime.AddDays(amount),
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
            "d" => isUpperLimit ? dateTime.EndOfDay() : dateTime.StartOfDay(),
            "h" or "H" => isUpperLimit ? dateTime.EndOfHour() : dateTime.StartOfHour(),
            "m" => isUpperLimit ? dateTime.EndOfMinute() : dateTime.StartOfMinute(),
            "s" => isUpperLimit ? dateTime.EndOfSecond() : dateTime.StartOfSecond(),
            _ => throw new ArgumentException($"Invalid time unit for rounding: {unit}")
        };
    }
}
