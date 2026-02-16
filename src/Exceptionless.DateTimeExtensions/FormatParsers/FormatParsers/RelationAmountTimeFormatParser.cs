using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers;

[Priority(10)]
public partial class RelationAmountTimeFormatParser : IFormatParser
{
    [GeneratedRegex(@"^\s*(?<relation>" + Helper.RelationNames + @")\s+(?<amount>\d+)\s+(?<size>" + Helper.AllTimeNames + @")\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex Parser();

    public virtual DateTimeRange? Parse(string content, DateTimeOffset relativeBaseTime)
    {
        var m = Parser().Match(content);
        if (!m.Success)
            return null;

        return FromRelationAmountTime(m.Groups["relation"].Value, Int32.Parse(m.Groups["amount"].Value), m.Groups["size"].Value, relativeBaseTime);
    }

    protected DateTimeRange? FromRelationAmountTime(string relation, int amount, string size, DateTimeOffset relativeBaseTime)
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
                "last" or "past" or "previous" => new DateTimeRange(relativeBaseTime.Floor(intervalSpan).SafeSubtract(totalSpan), relativeBaseTime),
                "this" or "next" => new DateTimeRange(relativeBaseTime, relativeBaseTime.SafeAdd(totalSpan).Ceiling(intervalSpan).SubtractMilliseconds(1)),
                _ => null
            };
        }

        if (String.Equals(size, "week", StringComparison.OrdinalIgnoreCase) || String.Equals(size, "weeks", StringComparison.OrdinalIgnoreCase))
        {
            return relation switch
            {
                "last" or "past" or "previous" => new DateTimeRange(relativeBaseTime.SubtractWeeks(amount).StartOfDay(), relativeBaseTime),
                "this" or "next" => new DateTimeRange(relativeBaseTime, relativeBaseTime.AddWeeks(amount).EndOfDay()),
                _ => null
            };
        }

        if (String.Equals(size, "month", StringComparison.OrdinalIgnoreCase) || String.Equals(size, "months", StringComparison.OrdinalIgnoreCase))
        {
            return relation switch
            {
                "last" or "past" or "previous" => new DateTimeRange(relativeBaseTime.SubtractMonths(amount).StartOfDay(), relativeBaseTime),
                "this" or "next" => new DateTimeRange(relativeBaseTime, relativeBaseTime.AddMonths(amount).EndOfDay()),
                _ => null
            };
        }

        if (String.Equals(size, "year", StringComparison.OrdinalIgnoreCase) || String.Equals(size, "years", StringComparison.OrdinalIgnoreCase))
        {
            return relation switch
            {
                "last" or "past" or "previous" => new DateTimeRange(relativeBaseTime.SubtractYears(amount).StartOfDay(), relativeBaseTime),
                "this" or "next" => new DateTimeRange(relativeBaseTime, relativeBaseTime.AddYears(amount).EndOfDay()),
                _ => null
            };
        }

        return null;
    }
}
