using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;

[Priority(1)]
public class WildcardPartParser : IPartParser
{
    private static readonly Regex _wildCardRegex = new(@"\G\s*\*(?=\s|\]|\}|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public Regex Regex => _wildCardRegex;

    public DateTimeOffset? Parse(Match match, DateTimeOffset relativeBaseTime, bool isUpperLimit)
    {
        if (!match.Success)
            return null;

        return isUpperLimit ? DateTimeOffset.MaxValue : DateTimeOffset.MinValue;
    }
}
