using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers {
    [Priority(70)]
    public class YearPartParser : IPartParser {
        private static readonly Regex _parser = new Regex(@"\G(?<year>\d{4})");
        public Regex Regex { get { return _parser; } }

        public DateTime? Parse(Match match, DateTime now, bool isUpperLimit) {
            int year = Int32.Parse(match.Groups["year"].Value);
            return isUpperLimit ? now.ChangeYear(year).EndOfYear() : now.ChangeYear(year).StartOfYear();
        }
    }
}