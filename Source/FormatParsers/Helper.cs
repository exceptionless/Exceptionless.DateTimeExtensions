using System;
using System.Collections.Generic;
using System.Linq;

namespace Exceptionless.DateTimeExtensions.FormatParsers {
    internal static class Helper {
        internal static readonly string SingularTimeNames = "minute|hour|day|week|month|year";
        internal static readonly string PluralTimeNames = "minutes|hours|days|weeks|months|years";
        internal static readonly string AllTimeNames = SingularTimeNames + "|" + PluralTimeNames;
        internal static readonly string RelationNames = "this|past|last|next|previous";
        internal static readonly List<string> MonthNames = new List<string>(new[] { "january", "february", "march", "april", "may", "june", "july", "august", "september", "october", "november", "december" });

        internal static TimeSpan GetTimeSpanFromName(string name) {
            switch (name.ToLower()) {
            case "minutes":
            case "minute":
                return TimeSpan.FromMinutes(1);
            case "hours":
            case "hour":
                return TimeSpan.FromHours(1);
            case "days":
            case "day":
                return TimeSpan.FromDays(1);
            default:
                return TimeSpan.Zero;
            }
        }

        internal static int GetMonthNumber(string name) {
            int index = MonthNames.FindIndex(m => m.Equals(name, StringComparison.OrdinalIgnoreCase) || m.Substring(0, 3).Equals(name, StringComparison.OrdinalIgnoreCase));
            return index >= 0 ? index + 1 : -1;
        }

        internal static string GetMonthNames() {
            return String.Join("|", MonthNames) + "|" + String.Join("|", MonthNames.Where(m => m.Length > 3).Select(m => m.Substring(0, 3)));
        }
    }
}
