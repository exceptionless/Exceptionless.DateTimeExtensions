using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;

[Priority(30)]
public class MonthRelationPartParser : IPartParser
{
    private static readonly Regex _parser = new(String.Format(@"\G(?<relation>{0})\s+(?<month>{1})", Helper.RelationNames, Helper.GetMonthNames()), RegexOptions.IgnoreCase);
    public virtual Regex Regex => _parser;

    public virtual DateTimeOffset? Parse(Match match, DateTimeOffset relativeBaseTime, bool isUpperLimit)
    {
        string relation = match.Groups["relation"].Value.ToLower();
        int month = Helper.GetMonthNumber(match.Groups["month"].Value);
        return FromMonthRelation(relation, month, relativeBaseTime, isUpperLimit);
    }

    protected DateTimeOffset? FromMonthRelation(string relation, int month, DateTimeOffset relativeBaseTime, bool isUpperLimit)
    {
        switch (relation)
        {
            case "this":
                {
                    var start = relativeBaseTime.Month == month ? relativeBaseTime.StartOfMonth() : relativeBaseTime.Month < month ? relativeBaseTime.StartOfMonth().ChangeMonth(month) : relativeBaseTime.StartOfMonth().AddYears(1).ChangeMonth(month);
                    return isUpperLimit ? start.EndOfMonth() : start;
                }
            case "last":
            case "past":
            case "previous":
                {
                    var start = relativeBaseTime.Month > month ? relativeBaseTime.StartOfMonth().ChangeMonth(month) : relativeBaseTime.StartOfMonth().SubtractYears(1).ChangeMonth(month);
                    return isUpperLimit ? start.EndOfMonth() : start;
                }
            case "next":
                {
                    var start = relativeBaseTime.Month < month ? relativeBaseTime.StartOfMonth().ChangeMonth(month) : relativeBaseTime.StartOfMonth().AddYears(1).ChangeMonth(month);
                    return isUpperLimit ? start.EndOfMonth() : start;
                }
        }

        return null;
    }
}
