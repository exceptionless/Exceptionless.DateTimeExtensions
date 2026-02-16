using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers;

[Priority(70)]
public partial class SingleTimeRelationFormatParser : RelationAmountTimeFormatParser
{
    [GeneratedRegex(@"^\s*(?<relation>" + Helper.RelationNames + @")\s+(?<time>" + Helper.SingularTimeNames + @")\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex SingleTimeParser();

    public override DateTimeRange? Parse(string content, DateTimeOffset relativeBaseTime)
    {
        var m = SingleTimeParser().Match(content);
        if (!m.Success)
            return null;

        return FromRelationAmountTime(m.Groups["relation"].Value, 1, m.Groups["time"].Value, relativeBaseTime);
    }
}
