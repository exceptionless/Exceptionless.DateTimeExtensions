using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;

[Priority(20)]
public class AmountTimeRelationPartParser : IPartParser
{
    private static readonly Regex _parser = new(String.Format(@"\G(?<amount>\d+)\s+(?<size>{0})\s+(?<relation>ago|from now)", Helper.AllTimeNames), RegexOptions.Compiled | RegexOptions.IgnoreCase);
    public virtual Regex Regex => _parser;

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
            switch (relation)
            {
                case "ago":
                    return isUpperLimit ? relativeBaseTime.SafeSubtract(totalSpan).Ceiling(intervalSpan).SubtractMilliseconds(1) : relativeBaseTime.SafeSubtract(totalSpan).Floor(intervalSpan);
                case "from now":
                    return isUpperLimit ? relativeBaseTime.SafeAdd(totalSpan).Ceiling(intervalSpan).SubtractMilliseconds(1) : relativeBaseTime.SafeAdd(totalSpan).Floor(intervalSpan);
            }
        }
        else if (String.Equals(size, "week", StringComparison.OrdinalIgnoreCase) || String.Equals(size, "weeks", StringComparison.OrdinalIgnoreCase))
        {
            switch (relation)
            {
                case "ago":
                    return isUpperLimit ? relativeBaseTime.SubtractWeeks(amount).EndOfDay() : relativeBaseTime.SubtractWeeks(amount).StartOfDay();
                case "from now":
                    return isUpperLimit ? relativeBaseTime.AddWeeks(amount).EndOfDay() : relativeBaseTime.AddWeeks(amount).StartOfDay();
            }
        }
        else if (String.Equals(size, "month", StringComparison.OrdinalIgnoreCase) || String.Equals(size, "months", StringComparison.OrdinalIgnoreCase))
        {
            switch (relation)
            {
                case "ago":
                    return isUpperLimit ? relativeBaseTime.SubtractMonths(amount).EndOfDay() : relativeBaseTime.SubtractMonths(amount).StartOfDay();
                case "from now":
                    return isUpperLimit ? relativeBaseTime.AddMonths(amount).EndOfDay() : relativeBaseTime.AddMonths(amount).StartOfDay();
            }
        }
        else if (String.Equals(size, "year", StringComparison.OrdinalIgnoreCase) || String.Equals(size, "years", StringComparison.OrdinalIgnoreCase))
        {
            switch (relation)
            {
                case "ago":
                    return isUpperLimit ? relativeBaseTime.SubtractYears(amount).EndOfDay() : relativeBaseTime.SubtractYears(amount).StartOfDay();
                case "from now":
                    return isUpperLimit ? relativeBaseTime.AddYears(amount).EndOfDay() : relativeBaseTime.AddYears(amount).StartOfDay();
            }
        }

        return null;
    }
}
