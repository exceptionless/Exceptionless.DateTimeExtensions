using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers {
    [Priority(30)]
    public class ExplicitDateFormatParser : IFormatParser {
        private static readonly Regex _parser = new(@"^\s*(?<date>\d{4}-\d{2}-\d{2}(?:T(?:\d{2}\:\d{2}\:\d{2}|\d{2}\:\d{2}|\d{2}))?)\s*$");

        public DateTimeRange Parse(string content, DateTimeOffset relativeBaseTime) {
            content = content.Trim();
            var m = _parser.Match(content);
            if (!m.Success)
                return null;

            string value = m.Groups["date"].Value;
            if (value.Length == 13)
                value += ":00:00";
            if (value.Length == 16)
                value += ":00";

            if (!DateTimeOffset.TryParse(value, out var date))
                return null;

            date = date.ChangeOffset(relativeBaseTime.Offset);
            switch (content.Length) {
                case 10:
                    return new DateTimeRange(date, date.EndOfDay());
                case 13:
                    return new DateTimeRange(date, date.EndOfHour());
                case 16:
                    return new DateTimeRange(date, date.EndOfMinute());
                default:
                    return new DateTimeRange(date, date.EndOfSecond());
            }
        }
    }
}