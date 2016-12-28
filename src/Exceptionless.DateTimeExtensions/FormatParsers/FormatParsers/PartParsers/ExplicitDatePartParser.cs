using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers {
    [Priority(50)]
    public class ExplicitDatePartParser : IPartParser {
        private static readonly Regex _parser = new Regex(@"\G(?<date>\d{4}-\d{2}-\d{2}(?:T(?:\d{2}\:\d{2}\:\d{2}|\d{2}\:\d{2}|\d{2}))?)");
        public Regex Regex => _parser;

        public DateTimeOffset? Parse(Match match, DateTimeOffset relativeBaseTime, bool isUpperLimit) {
            string value = match.Groups["date"].Value;
            if (value.Length == 13)
                value += ":00:00";
            if (value.Length == 16)
                value += ":00";

            DateTimeOffset date;
            if (!DateTimeOffset.TryParse(value, out date))
                return null;

            date = date.ChangeOffset(relativeBaseTime.Offset);
            if (!isUpperLimit)
                return date;

            switch (match.Length) {
                case 10:
                    return date.EndOfDay();
                case 13:
                    return  date.EndOfHour();
                case 16:
                    return date.EndOfMinute();
                default:
                    return date;
            }
        }
    }
}