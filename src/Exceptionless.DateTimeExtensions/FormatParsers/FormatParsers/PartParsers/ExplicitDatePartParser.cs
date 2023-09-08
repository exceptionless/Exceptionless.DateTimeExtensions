using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers {
    [Priority(50)]
    public class ExplicitDatePartParser : IPartParser {
        private static readonly Regex _parser = new(@"\G(?<date>\d{4}-\d{2}-\d{2}(?:T(?:\d{2}\:\d{2}\:\d{2}|\d{2}\:\d{2}|\d{2}))?)");
        public Regex Regex => _parser;

        public DateTimeOffset? Parse(Match match, DateTimeOffset relativeBaseTime, bool isUpperLimit) {
            string value = match.Groups["date"].Value;
            if (value.Length == 13)
                value += ":00:00";
            if (value.Length == 16)
                value += ":00";

            if (!DateTimeOffset.TryParse(value, out var date))
                return null;

            date = date.ChangeOffset(relativeBaseTime.Offset);
            if (!isUpperLimit)
                return date;

            return match.Length switch {
                10 => date.EndOfDay(),
                13 => date.EndOfHour(),
                16 => date.EndOfMinute(),
                _ => date
            };
        }
    }
}