using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers {
    [Priority(60)]
    public class MonthDayPartParser : IPartParser {
        private static readonly Regex _parser = new Regex(@"\G(?<month>\d{2})-(?<day>\d{2})");
        public Regex Regex { get { return _parser; } }

        public DateTime? Parse(Match match, DateTime now, bool isUpperLimit) {
            int month = Int32.Parse(match.Groups["month"].Value);
            int day = Int32.Parse(match.Groups["day"].Value);

            try {
                return isUpperLimit ? now.Date.ChangeMonth(month).ChangeDay(day).EndOfDay() : now.Date.ChangeMonth(month).ChangeDay(day);
            } catch {
                return null;
            }
        }
    }
}