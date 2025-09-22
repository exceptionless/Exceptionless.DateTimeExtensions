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
        for (int i = 0; i < value.Length; i++)
            if (!Char.IsLetterOrDigit(value[i]) && value[i] != '-' && value[i] != '+' && !Char.IsWhiteSpace(value[i]))
                return null;

        // compare using the original value as uppercase M could mean months.
        var normalized = value.Trim();
        if (value.EndsWith("m") && Int32.TryParse(normalized.Substring(0, normalized.Length - 1), out var minutes))
            return new TimeSpan(0, minutes, 0);

        if (normalized.EndsWith("h") && Int32.TryParse(normalized.Substring(0, normalized.Length - 1), out var hours))
            return new TimeSpan(hours, 0, 0);

        if (normalized.EndsWith("d") && Int32.TryParse(normalized.Substring(0, normalized.Length - 1), out var days))
            return new TimeSpan(days, 0, 0, 0);

        if (normalized.EndsWith("nanos", StringComparison.OrdinalIgnoreCase) && Int32.TryParse(normalized.Substring(0, normalized.Length - 5), out var nanoseconds))
            return new TimeSpan((int)Math.Round(nanoseconds / 100d));

        if (normalized.EndsWith("micros", StringComparison.OrdinalIgnoreCase) && Int32.TryParse(normalized.Substring(0, normalized.Length - 6), out var microseconds))
            return new TimeSpan(microseconds * 10);

        if (normalized.EndsWith("ms") && Int32.TryParse(normalized.Substring(0, normalized.Length - 2), out var milliseconds))
            return new TimeSpan(0, 0, 0, 0, milliseconds);

        if (normalized.EndsWith("s") && Int32.TryParse(normalized.Substring(0, normalized.Length - 1), out var seconds))
            return new TimeSpan(0, 0, seconds);

        return null;
    }
}
