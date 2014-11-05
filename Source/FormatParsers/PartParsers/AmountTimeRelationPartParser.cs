using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers {
    [Priority(20)]
    public class AmountTimeRelationPartParser : IPartParser {
        private static readonly Regex _parser = new Regex(String.Format(@"\G(?<amount>\d+)\s+(?<size>{0})\s+(?<relation>ago|from now)", Helper.AllTimeNames), RegexOptions.IgnoreCase);
        public virtual Regex Regex { get { return _parser; } }

        public virtual DateTime? Parse(Match match, DateTime now, bool isUpperLimit) {
            return FromRelationAmountTime(
                match.Groups["relation"].Value,
                Int32.Parse(match.Groups["amount"].Value),
                match.Groups["size"].Value,
                now,
                isUpperLimit);
        }

        protected DateTime? FromRelationAmountTime(string relation, int amount, string size, DateTime now, bool isUpperLimit) {
            relation = relation.ToLower();
            size = size.ToLower();
            if (amount < 1)
                throw new ArgumentException("Time amount can't be 0.");
            TimeSpan intervalSpan = Helper.GetTimeSpanFromName(size);

            if (intervalSpan != TimeSpan.Zero) {
                var totalSpan = TimeSpan.FromTicks(intervalSpan.Ticks * amount);
                switch (relation) {
                case "ago":
                    return isUpperLimit ? now.Subtract(totalSpan).Ceiling(intervalSpan).SubtractMilliseconds(1) : now.Subtract(totalSpan).Floor(intervalSpan);
                case "from now":
                    return isUpperLimit ? now.Add(totalSpan).Ceiling(intervalSpan).SubtractMilliseconds(1) : now.Add(totalSpan).Floor(intervalSpan);
                }
            } else if (size == "week" || size == "weeks") {
                switch (relation) {
                case "ago":
                    return isUpperLimit ? now.SubtractWeeks(amount).EndOfDay() : now.SubtractWeeks(amount).StartOfDay();
                case "from now":
                    return isUpperLimit ? now.AddWeeks(amount).EndOfDay() : now.AddWeeks(amount).StartOfDay();
                }
            } else if (size == "month" || size == "months") {
                switch (relation) {
                case "ago":
                    return isUpperLimit ? now.SubtractMonths(amount).EndOfDay() : now.SubtractMonths(amount).StartOfDay();
                case "from now":
                    return isUpperLimit ? now.AddMonths(amount).EndOfDay() : now.AddMonths(amount).StartOfDay();
                }
            } else if (size == "year" || size == "years") {
                switch (relation) {
                case "ago":
                    return isUpperLimit ? now.SubtractYears(amount).EndOfDay() : now.SubtractYears(amount).StartOfDay();
                case "from now":
                    return isUpperLimit ? now.AddYears(amount).EndOfDay() : now.AddYears(amount).StartOfDay();
                }
            }

            return null;
        }
    }
}
