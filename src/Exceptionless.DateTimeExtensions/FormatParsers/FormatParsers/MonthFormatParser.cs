using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers;

[Priority(60)]
public class MonthFormatParser : MonthRelationFormatParser
{
    private static readonly Regex _parser = new(String.Format(@"^\s*(?<month>{0})\s*$", Helper.GetMonthNames()), RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public override DateTimeRange Parse(string content, DateTimeOffset relativeBaseTime)
    {
        var m = _parser.Match(content);
        if (!m.Success)
            return null;

        int month = Helper.GetMonthNumber(m.Groups["month"].Value);
        string relation = relativeBaseTime.Month > month ? "last" : "this";
        return FromMonthRelation(relation, month, relativeBaseTime);
    }
}
