using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers {
    [Priority(40)]
    public class MonthPartParser : MonthRelationPartParser {
        private static readonly Regex _parser = new Regex(String.Format(@"\G(?<month>{0})", Helper.GetMonthNames()), RegexOptions.IgnoreCase);
        public override Regex Regex { get { return _parser; } }

        public override DateTime? Parse(Match match, DateTime now, bool isUpperLimit) {
            int month = Helper.GetMonthNumber(match.Groups["month"].Value);
            string relation = now.Month > month ? "last" : "this";
            return FromMonthRelation(relation, month, now, isUpperLimit);
        }
    }
}