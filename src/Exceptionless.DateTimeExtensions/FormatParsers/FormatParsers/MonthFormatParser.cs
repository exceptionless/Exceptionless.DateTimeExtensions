using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers;

[Priority(60)]
public partial class MonthFormatParser : MonthRelationFormatParser
{
    [GeneratedRegex(@"^\s*(?<month>" + Helper.MonthNamesPattern + @")\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex MonthParser();

    public override DateTimeRange? Parse(string content, DateTimeOffset relativeBaseTime)
    {
        var m = MonthParser().Match(content);
        if (!m.Success)
            return null;

        int month = Helper.GetMonthNumber(m.Groups["month"].Value);
        string relation = relativeBaseTime.Month > month ? "last" : "this";
        return FromMonthRelation(relation, month, relativeBaseTime);
    }
}
