using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers {
    [Priority(50)]
    public class ExplicitDatePartParser : IPartParser {
        private static readonly Regex _parser = new Regex(@"\G(?<date>\d{4}-\d{2}-\d{2}(?:T(?:\d{2}\:\d{2}\:\d{2}|\d{2}\:\d{2}|\d{2}))?)");
        public Regex Regex { get { return _parser; } }

        public DateTime? Parse(Match match, DateTime now, bool isUpperLimit) {
            string value = match.Groups["date"].Value;
            if (value.Length == 13)
                value += ":00:00";
            if (value.Length == 16)
                value += ":00";

            DateTime date;
            if (!DateTime.TryParse(value, out date))
                return null;

            if (match.Length == 10)
                return isUpperLimit ? date.EndOfDay() : date;
            if (match.Length == 13)
                return isUpperLimit ? date.EndOfHour() : date;
            if (match.Length == 16)
                return isUpperLimit ? date.EndOfMinute() : date;

            return date;
        }
    }
}