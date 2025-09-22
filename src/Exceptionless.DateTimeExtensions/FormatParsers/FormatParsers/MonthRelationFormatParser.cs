using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers;

[Priority(40)]
public class MonthRelationFormatParser : IFormatParser
{
    private static readonly Regex _parser = new(String.Format(@"^\s*(?<relation>{0})\s+(?<month>{1})\s*$", Helper.RelationNames, Helper.GetMonthNames()), RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public virtual DateTimeRange Parse(string content, DateTimeOffset relativeBaseTime)
    {
        var m = _parser.Match(content);
        if (!m.Success)
            return null;

        string relation = m.Groups["relation"].Value.ToLower();
        int month = Helper.GetMonthNumber(m.Groups["month"].Value);
        return FromMonthRelation(relation, month, relativeBaseTime);
    }

    protected DateTimeRange FromMonthRelation(string relation, int month, DateTimeOffset relativeBaseTime)
    {
        switch (relation)
        {
            case "this":
                {
                    var start = relativeBaseTime.Month == month ? relativeBaseTime.StartOfMonth() : relativeBaseTime.Month < month ? relativeBaseTime.StartOfMonth().ChangeMonth(month) : relativeBaseTime.StartOfMonth().AddYears(1).ChangeMonth(month);
                    return new DateTimeRange(start, start.EndOfMonth());
                }
            case "last":
            case "past":
            case "previous":
                {
                    var start = relativeBaseTime.Month > month ? relativeBaseTime.StartOfMonth().ChangeMonth(month) : relativeBaseTime.StartOfMonth().SubtractYears(1).ChangeMonth(month);
                    return new DateTimeRange(start, start.EndOfMonth());
                }
            case "next":
                {
                    var start = relativeBaseTime.Month < month ? relativeBaseTime.StartOfMonth().ChangeMonth(month) : relativeBaseTime.StartOfMonth().AddYears(1).ChangeMonth(month);
                    return new DateTimeRange(start, start.EndOfMonth());
                }
        }

        return null;
    }
}
