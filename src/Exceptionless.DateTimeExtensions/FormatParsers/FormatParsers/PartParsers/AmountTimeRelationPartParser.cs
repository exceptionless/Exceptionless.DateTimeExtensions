using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;

[Priority(20)]
public partial class AmountTimeRelationPartParser : IPartParser
{
    public virtual Regex Regex => Parser();

    public virtual DateTimeOffset? Parse(Match match, DateTimeOffset relativeBaseTime, bool isUpperLimit)
    {
        return FromRelationAmountTime(
            match.Groups["relation"].Value,
            Int32.Parse(match.Groups["amount"].Value),
            match.Groups["size"].Value,
            relativeBaseTime,
            isUpperLimit);
    }

    protected DateTimeOffset? FromRelationAmountTime(string relation, int amount, string size, DateTimeOffset relativeBaseTime, bool isUpperLimit)
    {
        relation = relation.ToLowerInvariant();
        if (amount < 1)
            throw new ArgumentException("Time amount can't be 0.");
        var intervalSpan = Helper.GetTimeSpanFromName(size);

        if (intervalSpan != TimeSpan.Zero)
        {
            var totalSpan = TimeSpan.FromTicks(intervalSpan.Ticks * amount);
            return relation switch
            {
                "ago" => isUpperLimit ? relativeBaseTime.SafeSubtract(totalSpan).Ceiling(intervalSpan).SubtractMilliseconds(1) : relativeBaseTime.SafeSubtract(totalSpan).Floor(intervalSpan),
                "from now" => isUpperLimit ? relativeBaseTime.SafeAdd(totalSpan).Ceiling(intervalSpan).SubtractMilliseconds(1) : relativeBaseTime.SafeAdd(totalSpan).Floor(intervalSpan),
                _ => null
            };
        }

        if (String.Equals(size, "week", StringComparison.OrdinalIgnoreCase) || String.Equals(size, "weeks", StringComparison.OrdinalIgnoreCase))
        {
            return relation switch
            {
                "ago" => isUpperLimit ? relativeBaseTime.SubtractWeeks(amount).EndOfDay() : relativeBaseTime.SubtractWeeks(amount).StartOfDay(),
                "from now" => isUpperLimit ? relativeBaseTime.AddWeeks(amount).EndOfDay() : relativeBaseTime.AddWeeks(amount).StartOfDay(),
                _ => null
            };
        }

        if (String.Equals(size, "month", StringComparison.OrdinalIgnoreCase) || String.Equals(size, "months", StringComparison.OrdinalIgnoreCase))
        {
            return relation switch
            {
                "ago" => isUpperLimit ? relativeBaseTime.SubtractMonths(amount).EndOfDay() : relativeBaseTime.SubtractMonths(amount).StartOfDay(),
                "from now" => isUpperLimit ? relativeBaseTime.AddMonths(amount).EndOfDay() : relativeBaseTime.AddMonths(amount).StartOfDay(),
                _ => null
            };
        }

        if (String.Equals(size, "year", StringComparison.OrdinalIgnoreCase) || String.Equals(size, "years", StringComparison.OrdinalIgnoreCase))
        {
            return relation switch
            {
                "ago" => isUpperLimit ? relativeBaseTime.SubtractYears(amount).EndOfDay() : relativeBaseTime.SubtractYears(amount).StartOfDay(),
                "from now" => isUpperLimit ? relativeBaseTime.AddYears(amount).EndOfDay() : relativeBaseTime.AddYears(amount).StartOfDay(),
                _ => null
            };
        }

        return null;
    }

    [GeneratedRegex(@"\G(?<amount>\d+)\s+(?<size>" + Helper.AllTimeNames + @")\s+(?<relation>ago|from now)", RegexOptions.IgnoreCase)]
    private static partial Regex Parser();
}
