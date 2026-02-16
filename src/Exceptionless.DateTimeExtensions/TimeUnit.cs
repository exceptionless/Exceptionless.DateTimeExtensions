namespace Exceptionless.DateTimeExtensions;

public static class TimeUnit
{
    public static TimeSpan Parse(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);

        var time = ParseTime(value);
        if (time.HasValue)
            return time.Value;

        throw new ArgumentException($"Unable to parse value '{value}' as a valid time value.");
    }

    public static bool TryParse(string? value, out TimeSpan? time)
    {
        time = null;
        if (String.IsNullOrEmpty(value))
            return false;

        time = ParseTime(value);
        return time.HasValue;
    }

    private static TimeSpan? ParseTime(string value)
    {
        if (String.IsNullOrWhiteSpace(value))
            return null;

        ReadOnlySpan<char> span = value.AsSpan().Trim();

        if (span.IsEmpty)
            return null;

        // bail if we have any weird characters
        foreach (char c in span)
            if (!Char.IsLetterOrDigit(c) && c != '-' && c != '+' && !Char.IsWhiteSpace(c))
                return null;

        // Handle years (y) - using average days in a year
        if (span[^1] == 'y' && Int32.TryParse(span[..^1], out int years))
            return new TimeSpan((int)(years * TimeSpanExtensions.AvgDaysInAYear), 0, 0, 0);

        // Handle months (M) - using average days in a month, case-sensitive uppercase M
        if (span[^1] == 'M' && Int32.TryParse(span[..^1], out int months))
            return new TimeSpan((int)(months * TimeSpanExtensions.AvgDaysInAMonth), 0, 0, 0);

        // Handle weeks (w)
        if (span[^1] == 'w' && Int32.TryParse(span[..^1], out int weeks))
            return new TimeSpan(weeks * 7, 0, 0, 0);

        // Handle minutes (m) - lowercase m for minutes
        if (span[^1] == 'm' && Int32.TryParse(span[..^1], out int minutes))
            return new TimeSpan(0, minutes, 0);

        if (span[^1] == 'h' && Int32.TryParse(span[..^1], out int hours))
            return new TimeSpan(hours, 0, 0);

        if (span[^1] == 'd' && Int32.TryParse(span[..^1], out int days))
            return new TimeSpan(days, 0, 0, 0);

        if (span.EndsWith("nanos", StringComparison.OrdinalIgnoreCase) && Int32.TryParse(span[..^5], out int nanoseconds))
            return new TimeSpan((int)Math.Round(nanoseconds / 100d));

        if (span.EndsWith("micros", StringComparison.OrdinalIgnoreCase) && Int32.TryParse(span[..^6], out int microseconds))
            return new TimeSpan(microseconds * 10);

        if (span.EndsWith("ms") && Int32.TryParse(span[..^2], out int milliseconds))
            return new TimeSpan(0, 0, 0, 0, milliseconds);

        if (span[^1] == 's' && Int32.TryParse(span[..^1], out int seconds))
            return new TimeSpan(0, 0, seconds);

        return null;
    }
}
