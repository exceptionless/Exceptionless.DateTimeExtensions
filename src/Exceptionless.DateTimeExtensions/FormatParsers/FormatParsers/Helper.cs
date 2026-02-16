namespace Exceptionless.DateTimeExtensions.FormatParsers;

internal static class Helper
{
    internal const string SingularTimeNames = "minute|hour|day|week|month|year";
    internal const string PluralTimeNames = "minutes|hours|days|weeks|months|years";
    internal const string AllTimeNames = $"{SingularTimeNames}|{PluralTimeNames}";
    internal const string RelationNames = "this|past|last|next|previous";

    internal const string MonthNamesPattern =
        "january|february|march|april|may|june|july|august|september|october|november|december"
        + "|jan|feb|mar|apr|jun|jul|aug|sep|oct|nov|dec";

    private static readonly IReadOnlyList<string> MonthNames =
    [
        "january", "february", "march", "april", "may", "june",
        "july", "august", "september", "october", "november", "december"
    ];

    internal static TimeSpan GetTimeSpanFromName(string name)
    {
        if (String.Equals(name, "minutes", StringComparison.OrdinalIgnoreCase) ||
            String.Equals(name, "minute", StringComparison.OrdinalIgnoreCase))
            return TimeSpan.FromMinutes(1);

        if (String.Equals(name, "hours", StringComparison.OrdinalIgnoreCase) ||
            String.Equals(name, "hour", StringComparison.OrdinalIgnoreCase))
            return TimeSpan.FromHours(1);

        if (String.Equals(name, "days", StringComparison.OrdinalIgnoreCase) ||
            String.Equals(name, "day", StringComparison.OrdinalIgnoreCase))
            return TimeSpan.FromDays(1);

        return TimeSpan.Zero;
    }

    internal static int GetMonthNumber(string name)
    {
        ReadOnlySpan<char> nameSpan = name.AsSpan();
        for (int i = 0; i < MonthNames.Count; i++)
        {
            if (MonthNames[i].AsSpan().Equals(nameSpan, StringComparison.OrdinalIgnoreCase) ||
                MonthNames[i].AsSpan(0, 3).Equals(nameSpan, StringComparison.OrdinalIgnoreCase))
                return i + 1;
        }

        return -1;
    }
}
