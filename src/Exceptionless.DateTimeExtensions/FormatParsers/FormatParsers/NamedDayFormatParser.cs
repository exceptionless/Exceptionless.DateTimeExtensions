using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers;

[Priority(20)]
public partial class NamedDayFormatParser : IFormatParser
{
    [GeneratedRegex(@"^\s*(?<name>today|yesterday|tomorrow)\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex Parser();

    public DateTimeRange? Parse(string content, DateTimeOffset relativeBaseTime)
    {
        var m = Parser().Match(content);
        if (!m.Success)
            return null;

        return m.Groups["name"].Value.ToLowerInvariant() switch
        {
            "today" => new DateTimeRange(relativeBaseTime.StartOfDay(), relativeBaseTime.EndOfDay()),
            "yesterday" => new DateTimeRange(relativeBaseTime.SubtractDays(1).StartOfDay(), relativeBaseTime.SubtractDays(1).EndOfDay()),
            "tomorrow" => new DateTimeRange(relativeBaseTime.AddDays(1).StartOfDay(), relativeBaseTime.AddDays(1).EndOfDay()),
            _ => null
        };
    }
}
