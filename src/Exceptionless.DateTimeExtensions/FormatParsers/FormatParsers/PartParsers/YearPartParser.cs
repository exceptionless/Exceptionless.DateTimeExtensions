using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers {
    [Priority(70)]
    public class YearPartParser : IPartParser {
        private static readonly Regex _parser = new(@"\G(?<year>\d{4})");
        public Regex Regex => _parser;

        public DateTimeOffset? Parse(Match match, DateTimeOffset relativeBaseTime, bool isUpperLimit) {
            int year = Int32.Parse(match.Groups["year"].Value);
            return isUpperLimit ? relativeBaseTime.ChangeYear(year).EndOfYear() : relativeBaseTime.ChangeYear(year).StartOfYear();
        }
    }
}