using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;

[Priority(1)]
public partial class WildcardPartParser : IPartParser
{
    public Regex Regex => Parser();

    public DateTimeOffset? Parse(Match match, DateTimeOffset relativeBaseTime, bool isUpperLimit)
    {
        if (!match.Success)
            return null;

        return isUpperLimit ? DateTimeOffset.MaxValue : DateTimeOffset.MinValue;
    }

    [GeneratedRegex(@"\G\s*\*(?=\s|\]|\}|$)", RegexOptions.IgnoreCase)]
    private static partial Regex Parser();
}
