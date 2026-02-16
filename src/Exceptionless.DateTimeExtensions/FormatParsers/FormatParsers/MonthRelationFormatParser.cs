using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers;

[Priority(40)]
public partial class MonthRelationFormatParser : IFormatParser
{
    [GeneratedRegex(@"^\s*(?<relation>" + Helper.RelationNames + @")\s+(?<month>" + Helper.MonthNamesPattern + @")\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex Parser();

    public virtual DateTimeRange? Parse(string content, DateTimeOffset relativeBaseTime)
    {
        var m = Parser().Match(content);
        if (!m.Success)
            return null;

        string relation = m.Groups["relation"].Value.ToLowerInvariant();
        int month = Helper.GetMonthNumber(m.Groups["month"].Value);
        return FromMonthRelation(relation, month, relativeBaseTime);
    }

    protected DateTimeRange? FromMonthRelation(string relation, int month, DateTimeOffset relativeBaseTime)
    {
        return relation switch
        {
            "this" => ThisMonth(month, relativeBaseTime),
            "last" or "past" or "previous" => PastMonth(month, relativeBaseTime),
            "next" => NextMonth(month, relativeBaseTime),
            _ => null
        };
    }

    private static DateTimeRange ThisMonth(int month, DateTimeOffset relativeBaseTime)
    {
        var start = relativeBaseTime.Month == month
            ? relativeBaseTime.StartOfMonth()
            : relativeBaseTime.Month < month
                ? relativeBaseTime.StartOfMonth().ChangeMonth(month)
                : relativeBaseTime.StartOfMonth().AddYears(1).ChangeMonth(month);
        return new DateTimeRange(start, start.EndOfMonth());
    }

    private static DateTimeRange PastMonth(int month, DateTimeOffset relativeBaseTime)
    {
        var start = relativeBaseTime.Month > month
            ? relativeBaseTime.StartOfMonth().ChangeMonth(month)
            : relativeBaseTime.StartOfMonth().SubtractYears(1).ChangeMonth(month);
        return new DateTimeRange(start, start.EndOfMonth());
    }

    private static DateTimeRange NextMonth(int month, DateTimeOffset relativeBaseTime)
    {
        var start = relativeBaseTime.Month < month
            ? relativeBaseTime.StartOfMonth().ChangeMonth(month)
            : relativeBaseTime.StartOfMonth().AddYears(1).ChangeMonth(month);
        return new DateTimeRange(start, start.EndOfMonth());
    }
}
