using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers;

[Priority(20)]
public class NamedDayFormatParser : IFormatParser
{
    private static readonly Regex _parser = new(@"^\s*(?<name>today|yesterday|tomorrow)\s*$", RegexOptions.IgnoreCase);

    public DateTimeRange Parse(string content, DateTimeOffset relativeBaseTime)
    {
        var m = _parser.Match(content);
        if (!m.Success)
            return null;

        string value = m.Groups["name"].Value.ToLower();
        if (value == "today")
            return new DateTimeRange(relativeBaseTime.StartOfDay(), relativeBaseTime.EndOfDay());
        if (value == "yesterday")
            return new DateTimeRange(relativeBaseTime.SubtractDays(1).StartOfDay(), relativeBaseTime.SubtractDays(1).EndOfDay());
        if (value == "tomorrow")
            return new DateTimeRange(relativeBaseTime.AddDays(1).StartOfDay(), relativeBaseTime.AddDays(1).EndOfDay());

        return null;
    }
}
