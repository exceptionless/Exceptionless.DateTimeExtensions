using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers;

[Priority(70)]
public class SingleTimeRelationFormatParser : RelationAmountTimeFormatParser
{
    private static readonly Regex _parser = new(String.Format(@"^\s*(?<relation>{0})\s+(?<time>{1})\s*$", Helper.RelationNames, Helper.SingularTimeNames), RegexOptions.IgnoreCase);

    public override DateTimeRange Parse(string content, DateTimeOffset relativeBaseTime)
    {
        var m = _parser.Match(content);
        if (!m.Success)
            return null;

        return FromRelationAmountTime(m.Groups["relation"].Value, 1, m.Groups["time"].Value, relativeBaseTime);
    }
}
