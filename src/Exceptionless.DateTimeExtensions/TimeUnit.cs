using System;

namespace Exceptionless.DateTimeExtensions;

public static class TimeUnit
{
    public static TimeSpan Parse(string value)
    {
        if (String.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(value));

        var time = ParseTime(value);
        if (time.HasValue)
            return time.Value;

        throw new ArgumentException($"Unable to parse value '{value}' as a valid time value.");
    }

    public static bool TryParse(string value, out TimeSpan? time)
    {
        time = null;
        if (String.IsNullOrEmpty(value))
            return false;

        time = ParseTime(value);
        return time.HasValue;
    }

    private static TimeSpan? ParseTime(string value)
    {
        // bail if we have any weird characters
        foreach (char c in value)
            if (!Char.IsLetterOrDigit(c) && c != '-' && c != '+' && !Char.IsWhiteSpace(c))
                return null;

        // compare using the original value as uppercase M could mean months.
        string normalized = value.Trim();
        if (value.EndsWith("m") && Int32.TryParse(normalized.Substring(0, normalized.Length - 1), out int minutes))
            return new TimeSpan(0, minutes, 0);

        if (normalized.EndsWith("h") && Int32.TryParse(normalized.Substring(0, normalized.Length - 1), out int hours))
            return new TimeSpan(hours, 0, 0);

        if (normalized.EndsWith("d") && Int32.TryParse(normalized.Substring(0, normalized.Length - 1), out int days))
            return new TimeSpan(days, 0, 0, 0);

        if (normalized.EndsWith("nanos", StringComparison.OrdinalIgnoreCase) && Int32.TryParse(normalized.Substring(0, normalized.Length - 5), out int nanoseconds))
            return new TimeSpan((int)Math.Round(nanoseconds / 100d));

        if (normalized.EndsWith("micros", StringComparison.OrdinalIgnoreCase) && Int32.TryParse(normalized.Substring(0, normalized.Length - 6), out int microseconds))
            return new TimeSpan(microseconds * 10);

        if (normalized.EndsWith("ms") && Int32.TryParse(normalized.Substring(0, normalized.Length - 2), out int milliseconds))
            return new TimeSpan(0, 0, 0, 0, milliseconds);

        if (normalized.EndsWith("s") && Int32.TryParse(normalized.Substring(0, normalized.Length - 1), out int seconds))
            return new TimeSpan(0, 0, seconds);

        return null;
    }
}
