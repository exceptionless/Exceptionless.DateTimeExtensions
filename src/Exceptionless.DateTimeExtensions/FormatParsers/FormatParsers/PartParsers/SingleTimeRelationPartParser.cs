using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;

[Priority(80)]
public class SingleTimeRelationPartParser : AmountTimeRelationPartParser
{
    private static readonly Regex _parser = new(String.Format(@"\G(?:a|an)\s+(?<time>{0})\s+(?<relation>ago|from now)", Helper.SingularTimeNames), RegexOptions.Compiled | RegexOptions.IgnoreCase);
    public override Regex Regex => _parser;

    public override DateTimeOffset? Parse(Match match, DateTimeOffset relativeBaseTime, bool isUpperLimit)
    {
        return FromRelationAmountTime(
                match.Groups["relation"].Value,
                1,
                match.Groups["time"].Value,
                relativeBaseTime,
                isUpperLimit);
    }
}
