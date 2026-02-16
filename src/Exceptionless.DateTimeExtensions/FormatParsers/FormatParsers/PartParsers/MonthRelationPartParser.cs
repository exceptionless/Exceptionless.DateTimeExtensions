using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;

[Priority(30)]
public partial class MonthRelationPartParser : IPartParser
{
    public virtual Regex Regex => Parser();

    public virtual DateTimeOffset? Parse(Match match, DateTimeOffset relativeBaseTime, bool isUpperLimit)
    {
        string relation = match.Groups["relation"].Value.ToLowerInvariant();
        int month = Helper.GetMonthNumber(match.Groups["month"].Value);
        return FromMonthRelation(relation, month, relativeBaseTime, isUpperLimit);
    }

    protected DateTimeOffset? FromMonthRelation(string relation, int month, DateTimeOffset relativeBaseTime, bool isUpperLimit)
    {
        DateTimeOffset start;
        switch (relation)
        {
            case "this":
                start = relativeBaseTime.Month == month
                    ? relativeBaseTime.StartOfMonth()
                    : relativeBaseTime.Month < month
                        ? relativeBaseTime.StartOfMonth().ChangeMonth(month)
                        : relativeBaseTime.StartOfMonth().AddYears(1).ChangeMonth(month);
                return isUpperLimit ? start.EndOfMonth() : start;
            case "last":
            case "past":
            case "previous":
                start = relativeBaseTime.Month > month
                    ? relativeBaseTime.StartOfMonth().ChangeMonth(month)
                    : relativeBaseTime.StartOfMonth().SubtractYears(1).ChangeMonth(month);
                return isUpperLimit ? start.EndOfMonth() : start;
            case "next":
                start = relativeBaseTime.Month < month
                    ? relativeBaseTime.StartOfMonth().ChangeMonth(month)
                    : relativeBaseTime.StartOfMonth().AddYears(1).ChangeMonth(month);
                return isUpperLimit ? start.EndOfMonth() : start;
        }

        return null;
    }

    [GeneratedRegex(@"\G(?<relation>" + Helper.RelationNames + @")\s+(?<month>" + Helper.MonthNamesPattern + @")", RegexOptions.IgnoreCase)]
    private static partial Regex Parser();
}
