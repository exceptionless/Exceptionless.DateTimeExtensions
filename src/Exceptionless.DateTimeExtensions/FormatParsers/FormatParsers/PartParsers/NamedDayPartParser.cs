using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;

[Priority(10)]
public partial class NamedDayPartParser : IPartParser
{
    public Regex Regex => Parser();

    public DateTimeOffset? Parse(Match match, DateTimeOffset relativeBaseTime, bool isUpperLimit)
    {
        return match.Groups["name"].Value.ToLowerInvariant() switch
        {
            "now" => relativeBaseTime,
            "today" => isUpperLimit ? relativeBaseTime.EndOfDay() : relativeBaseTime.StartOfDay(),
            "yesterday" => isUpperLimit ? relativeBaseTime.SubtractDays(1).EndOfDay() : relativeBaseTime.SubtractDays(1).StartOfDay(),
            "tomorrow" => isUpperLimit ? relativeBaseTime.AddDays(1).EndOfDay() : relativeBaseTime.AddDays(1).StartOfDay(),
            _ => null
        };
    }

    [GeneratedRegex(@"\G(?<name>now|today|yesterday|tomorrow)", RegexOptions.IgnoreCase)]
    private static partial Regex Parser();
}
