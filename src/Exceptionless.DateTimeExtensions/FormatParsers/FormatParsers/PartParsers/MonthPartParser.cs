using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;

[Priority(40)]
public partial class MonthPartParser : MonthRelationPartParser
{
    public override Regex Regex => Parser();

    public override DateTimeOffset? Parse(Match match, DateTimeOffset relativeBaseTime, bool isUpperLimit)
    {
        int month = Helper.GetMonthNumber(match.Groups["month"].Value);
        string relation = relativeBaseTime.Month > month ? "last" : "this";
        return FromMonthRelation(relation, month, relativeBaseTime, isUpperLimit);
    }

    [GeneratedRegex(@"\G(?<month>" + Helper.MonthNamesPattern + @")", RegexOptions.IgnoreCase)]
    private static partial Regex Parser();
}
