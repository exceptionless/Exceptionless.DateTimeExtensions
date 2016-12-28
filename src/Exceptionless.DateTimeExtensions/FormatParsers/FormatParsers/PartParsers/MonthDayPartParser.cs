using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers {
    [Priority(60)]
    public class MonthDayPartParser : IPartParser {
        private static readonly Regex _parser = new Regex(@"\G(?<month>\d{2})-(?<day>\d{2})");
        public Regex Regex => _parser;

        public DateTimeOffset? Parse(Match match, DateTimeOffset relativeBaseTime, bool isUpperLimit) {
            int month = Int32.Parse(match.Groups["month"].Value);
            int day = Int32.Parse(match.Groups["day"].Value);

            try {
                return isUpperLimit ? relativeBaseTime.ChangeMonth(month).ChangeDay(day).EndOfDay() : relativeBaseTime.ChangeMonth(month).ChangeDay(day).StartOfDay();
            } catch {
                return null;
            }
        }
    }
}