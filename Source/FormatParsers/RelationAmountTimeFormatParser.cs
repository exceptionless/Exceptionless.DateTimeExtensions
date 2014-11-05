using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers {
    [Priority(10)]
    public class RelationAmountTimeFormatParser : IFormatParser {
        private static readonly Regex _parser = new Regex(String.Format(@"^\s*(?<relation>{0})\s+(?<amount>\d+)\s+(?<size>{1})\s*$", Helper.RelationNames, Helper.AllTimeNames), RegexOptions.IgnoreCase);

        public virtual DateTimeRange Parse(string content, DateTime now) {
            var m = _parser.Match(content);
            if (!m.Success)
                return null;

            return FromRelationAmountTime(m.Groups["relation"].Value, Int32.Parse(m.Groups["amount"].Value), m.Groups["size"].Value, now);
        }

        protected DateTimeRange FromRelationAmountTime(string relation, int amount, string size, DateTime now) {
            relation = relation.ToLower();
            size = size.ToLower();
            if (amount < 1)
                throw new ArgumentException("Time amount can't be 0.");

            TimeSpan intervalSpan = Helper.GetTimeSpanFromName(size);

            if (intervalSpan != TimeSpan.Zero) {
                var totalSpan = TimeSpan.FromTicks(intervalSpan.Ticks * amount);
                switch (relation) {
                    case "last":
                    case "past":
                    case "previous":
                        return new DateTimeRange(now.Floor(intervalSpan).Subtract(totalSpan), now);
                    case "this":
                    case "next":
                        return new DateTimeRange(now, now.Add(totalSpan).Ceiling(intervalSpan).SubtractMilliseconds(1));
                }
            } else if (size == "week" || size == "weeks") {
                switch (relation) {
                    case "last":
                    case "past":
                    case "previous":
                        return new DateTimeRange(now.SubtractWeeks(amount).StartOfDay(), now);
                    case "this":
                    case "next":
                        return new DateTimeRange(now, now.AddWeeks(amount).EndOfDay());
                }
            } else if (size == "month" || size == "months") {
                switch (relation) {
                    case "last":
                    case "past":
                    case "previous":
                        return new DateTimeRange(now.SubtractMonths(amount).StartOfDay(), now);
                    case "this":
                    case "next":
                        return new DateTimeRange(now, now.AddMonths(amount).EndOfDay());
                }
            } else if (size == "year" || size == "years") {
                switch (relation) {
                    case "last":
                    case "past":
                    case "previous":
                        return new DateTimeRange(now.SubtractYears(amount).StartOfDay(), now);
                    case "this":
                    case "next":
                        return new DateTimeRange(now, now.AddYears(amount).EndOfDay());
                }
            }

            return null;
        }
    }
}