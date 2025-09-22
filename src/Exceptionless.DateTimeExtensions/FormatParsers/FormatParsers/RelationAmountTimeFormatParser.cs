using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers;

[Priority(10)]
public class RelationAmountTimeFormatParser : IFormatParser
{
    private static readonly Regex _parser = new(String.Format(@"^\s*(?<relation>{0})\s+(?<amount>\d+)\s+(?<size>{1})\s*$", Helper.RelationNames, Helper.AllTimeNames), RegexOptions.IgnoreCase);

    public virtual DateTimeRange Parse(string content, DateTimeOffset relativeBaseTime)
    {
        var m = _parser.Match(content);
        if (!m.Success)
            return null;

        return FromRelationAmountTime(m.Groups["relation"].Value, Int32.Parse(m.Groups["amount"].Value), m.Groups["size"].Value, relativeBaseTime);
    }

    protected DateTimeRange FromRelationAmountTime(string relation, int amount, string size, DateTimeOffset relativeBaseTime)
    {
        relation = relation.ToLower();
        size = size.ToLower();
        if (amount < 1)
            throw new ArgumentException("Time amount can't be 0.");

        var intervalSpan = Helper.GetTimeSpanFromName(size);

        if (intervalSpan != TimeSpan.Zero)
        {
            var totalSpan = TimeSpan.FromTicks(intervalSpan.Ticks * amount);
            switch (relation)
            {
                case "last":
                case "past":
                case "previous":
                    return new DateTimeRange(relativeBaseTime.Floor(intervalSpan).SafeSubtract(totalSpan), relativeBaseTime);
                case "this":
                case "next":
                    return new DateTimeRange(relativeBaseTime, relativeBaseTime.SafeAdd(totalSpan).Ceiling(intervalSpan).SubtractMilliseconds(1));
            }
        }
        else if (size == "week" || size == "weeks")
        {
            switch (relation)
            {
                case "last":
                case "past":
                case "previous":
                    return new DateTimeRange(relativeBaseTime.SubtractWeeks(amount).StartOfDay(), relativeBaseTime);
                case "this":
                case "next":
                    return new DateTimeRange(relativeBaseTime, relativeBaseTime.AddWeeks(amount).EndOfDay());
            }
        }
        else if (size == "month" || size == "months")
        {
            switch (relation)
            {
                case "last":
                case "past":
                case "previous":
                    return new DateTimeRange(relativeBaseTime.SubtractMonths(amount).StartOfDay(), relativeBaseTime);
                case "this":
                case "next":
                    return new DateTimeRange(relativeBaseTime, relativeBaseTime.AddMonths(amount).EndOfDay());
            }
        }
        else if (size == "year" || size == "years")
        {
            switch (relation)
            {
                case "last":
                case "past":
                case "previous":
                    return new DateTimeRange(relativeBaseTime.SubtractYears(amount).StartOfDay(), relativeBaseTime);
                case "this":
                case "next":
                    return new DateTimeRange(relativeBaseTime, relativeBaseTime.AddYears(amount).EndOfDay());
            }
        }

        return null;
    }
}
