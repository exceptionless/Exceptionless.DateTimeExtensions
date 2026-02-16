using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;

[Priority(80)]
public partial class SingleTimeRelationPartParser : AmountTimeRelationPartParser
{
    public override Regex Regex => Parser();

    public override DateTimeOffset? Parse(Match match, DateTimeOffset relativeBaseTime, bool isUpperLimit)
    {
        return FromRelationAmountTime(
                match.Groups["relation"].Value,
                1,
                match.Groups["time"].Value,
                relativeBaseTime,
                isUpperLimit);
    }

    [GeneratedRegex(@"\G(?:a|an)\s+(?<time>" + Helper.SingularTimeNames + @")\s+(?<relation>ago|from now)", RegexOptions.IgnoreCase)]
    private static partial Regex Parser();
}
