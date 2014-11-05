using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers {
    [Priority(40)]
    public class MonthRelationFormatParser : IFormatParser {
        private static readonly Regex _parser = new Regex(String.Format(@"^\s*(?<relation>{0})\s+(?<month>{1})\s*$", Helper.RelationNames, Helper.GetMonthNames()), RegexOptions.IgnoreCase);

        public virtual DateTimeRange Parse(string content, DateTime now) {
            var m = _parser.Match(content);
            if (!m.Success)
                return null;

            string relation = m.Groups["relation"].Value.ToLower();
            int month = Helper.GetMonthNumber(m.Groups["month"].Value);
            return FromMonthRelation(relation, month, now);
        }

        protected DateTimeRange FromMonthRelation(string relation, int month, DateTime now) {
            switch (relation) {
                case "this": {
                    var start = now.Month == month ? now.StartOfMonth() : now.Month < month ? now.StartOfMonth().ChangeMonth(month) : now.StartOfMonth().AddYears(1).ChangeMonth(month);
                    return new DateTimeRange(start, start.EndOfMonth());
                }
                case "last":
                case "past":
                case "previous": {
                    var start = now.Month > month ? now.StartOfMonth().ChangeMonth(month) : now.StartOfMonth().SubtractYears(1).ChangeMonth(month);
                    return new DateTimeRange(start, start.EndOfMonth());
                }
                case "next": {
                    var start = now.Month < month ? now.StartOfMonth().ChangeMonth(month) : now.StartOfMonth().AddYears(1).ChangeMonth(month);
                    return new DateTimeRange(start, start.EndOfMonth());
                }
            }

            return null;
        }
    }
}