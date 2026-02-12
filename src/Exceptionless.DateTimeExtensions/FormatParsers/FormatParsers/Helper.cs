using System;
using System.Collections.Generic;
using System.Linq;

namespace Exceptionless.DateTimeExtensions.FormatParsers;

internal static class Helper
{
    internal static readonly string SingularTimeNames = "minute|hour|day|week|month|year";
    internal static readonly string PluralTimeNames = "minutes|hours|days|weeks|months|years";
    internal static readonly string AllTimeNames = SingularTimeNames + "|" + PluralTimeNames;
    internal static readonly string RelationNames = "this|past|last|next|previous";
    internal static readonly List<string> MonthNames =
    [
        ..new[]
        {
            "january", "february", "march", "april", "may", "june", "july", "august", "september", "october",
            "november", "december"
        }
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
        int index = MonthNames.FindIndex(m =>
            String.Equals(m, name, StringComparison.OrdinalIgnoreCase) ||
            String.Equals(m.Substring(0, 3), name, StringComparison.OrdinalIgnoreCase));
        return index >= 0 ? index + 1 : -1;
    }

    internal static string GetMonthNames()
    {
        return String.Join("|", MonthNames) + "|" + String.Join("|", MonthNames.Where(m => m.Length > 3).Select(m => m.Substring(0, 3)));
    }
}
