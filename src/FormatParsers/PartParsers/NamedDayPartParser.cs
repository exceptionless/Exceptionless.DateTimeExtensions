using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers {
    [Priority(10)]
    public class NamedDayPartParser : IPartParser {
        private static readonly Regex _parser = new Regex(@"\G(?<name>now|today|yesterday|tomorrow)", RegexOptions.IgnoreCase);

        public Regex Regex { get { return _parser; } }

        public DateTime? Parse(Match match, DateTime now, bool isUpperLimit) {
            string value = match.Groups["name"].Value.ToLower();
            if (value == "now")
                return now;
            if (value == "today")
                return isUpperLimit ? now.Date.EndOfDay() : now.Date;
            if (value == "yesterday")
                return isUpperLimit ? now.Date.SubtractDays(1).EndOfDay() : now.SubtractDays(1).Date;
            if (value == "tomorrow")
                return isUpperLimit ? now.Date.AddDays(1).EndOfDay() : now.AddDays(1).Date;

            return null;
        }
    }
}