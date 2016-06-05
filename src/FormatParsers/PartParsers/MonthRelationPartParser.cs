using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers {
    [Priority(30)]
    public class MonthRelationPartParser : IPartParser {
        private static readonly Regex _parser = new Regex(String.Format(@"\G(?<relation>{0})\s+(?<month>{1})", Helper.RelationNames, Helper.GetMonthNames()), RegexOptions.IgnoreCase);
        public virtual Regex Regex { get { return _parser; } }

        public virtual DateTime? Parse(Match match, DateTime now, bool isUpperLimit) {
            string relation = match.Groups["relation"].Value.ToLower();
            int month = Helper.GetMonthNumber(match.Groups["month"].Value);
            return FromMonthRelation(relation, month, now, isUpperLimit);
        }

        protected DateTime? FromMonthRelation(string relation, int month, DateTime now, bool isUpperLimit) {
            switch (relation) {
                case "this": {
                    var start = now.Month == month ? now.StartOfMonth() : now.Month < month ? now.StartOfMonth().ChangeMonth(month) : now.StartOfMonth().AddYears(1).ChangeMonth(month);
                    return isUpperLimit ? start.EndOfMonth() : start;
                }
                case "last":
                case "past":
                case "previous": {
                    var start = now.Month > month ? now.StartOfMonth().ChangeMonth(month) : now.StartOfMonth().SubtractYears(1).ChangeMonth(month);
                    return isUpperLimit ? start.EndOfMonth() : start;
                }
                case "next": {
                    var start = now.Month < month ? now.StartOfMonth().ChangeMonth(month) : now.StartOfMonth().AddYears(1).ChangeMonth(month);
                    return isUpperLimit ? start.EndOfMonth() : start;
                }
            }

            return null;
        }
    }
}