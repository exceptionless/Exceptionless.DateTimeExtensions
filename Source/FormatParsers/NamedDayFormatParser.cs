using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers {
    [Priority(20)]
    public class NamedDayFormatParser : IFormatParser {
        private static readonly Regex _parser = new Regex(@"^\s*(?<name>today|yesterday|tomorrow)\s*$", RegexOptions.IgnoreCase);

        public DateTimeRange Parse(string content, DateTime now) {
            var m = _parser.Match(content);
            if (!m.Success)
                return null;

            string value = m.Groups["name"].Value.ToLower();
            if (value == "today")
                return new DateTimeRange(now.Date, now.EndOfDay());
            if (value == "yesterday")
                return new DateTimeRange(now.Date.SubtractDays(1), now.Date.SubtractDays(1).EndOfDay());
            if (value == "tomorrow")
                return new DateTimeRange(now.Date.AddDays(1), now.Date.AddDays(1).EndOfDay());

            return null;
        }
    }
}