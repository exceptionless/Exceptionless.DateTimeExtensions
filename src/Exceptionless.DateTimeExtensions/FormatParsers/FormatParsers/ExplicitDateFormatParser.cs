using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers {
    [Priority(30)]
    public class ExplicitDateFormatParser : IFormatParser {
        private static readonly Regex _parser = new Regex(@"^\s*(?<date>\d{4}-\d{2}-\d{2}(?:T(?:\d{2}\:\d{2}\:\d{2}|\d{2}\:\d{2}|\d{2}))?)\s*$");

        public DateTimeRange Parse(string content, DateTime now) {
            content = content.Trim();
            var m = _parser.Match(content);
            if (!m.Success)
                return null;

            string value = m.Groups["date"].Value;
            if (value.Length == 13)
                value += ":00:00";
            if (value.Length == 16)
                value += ":00";

            DateTime date;
            if (!DateTime.TryParse(value, out date))
                return null;

            if (content.Length == 10)
                return new DateTimeRange(date, date.EndOfDay());
            if (content.Length == 13)
                return new DateTimeRange(date, date.EndOfHour());
            if (content.Length == 16)
                return new DateTimeRange(date, date.EndOfMinute());

            return new DateTimeRange(date, date.EndOfSecond());
        }
    }
}