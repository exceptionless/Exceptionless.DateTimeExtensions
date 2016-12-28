using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers {
    [Priority(40)]
    public class MonthPartParser : MonthRelationPartParser {
        private static readonly Regex _parser = new Regex(String.Format(@"\G(?<month>{0})", Helper.GetMonthNames()), RegexOptions.IgnoreCase);
        public override Regex Regex => _parser;

        public override DateTimeOffset? Parse(Match match, DateTimeOffset relativeBaseTime, bool isUpperLimit) {
            int month = Helper.GetMonthNumber(match.Groups["month"].Value);
            string relation = relativeBaseTime.Month > month ? "last" : "this";
            return FromMonthRelation(relation, month, relativeBaseTime, isUpperLimit);
        }
    }
}