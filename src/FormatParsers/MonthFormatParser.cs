using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers {
    [Priority(60)]
    public class MonthFormatParser : MonthRelationFormatParser {
        private static readonly Regex _parser = new Regex(String.Format(@"^\s*(?<month>{0})\s*$", Helper.GetMonthNames()), RegexOptions.IgnoreCase);

        public override DateTimeRange Parse(string content, DateTime now) {
            var m = _parser.Match(content);
            if (!m.Success)
                return null;

            int month = Helper.GetMonthNumber(m.Groups["month"].Value);
            string relation = now.Month > month ? "last" : "this";
            return FromMonthRelation(relation, month, now);
        }
    }
}