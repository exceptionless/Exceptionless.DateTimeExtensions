using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers;

[Priority(20)]
public class NamedDayFormatParser : IFormatParser
{
    private static readonly Regex _parser = new(@"^\s*(?<name>today|yesterday|tomorrow)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public DateTimeRange Parse(string content, DateTimeOffset relativeBaseTime)
    {
        var m = _parser.Match(content);
        if (!m.Success)
            return null;

        string value = m.Groups["name"].Value;
        if (String.Equals(value, "today", StringComparison.OrdinalIgnoreCase))
            return new DateTimeRange(relativeBaseTime.StartOfDay(), relativeBaseTime.EndOfDay());
        if (String.Equals(value, "yesterday", StringComparison.OrdinalIgnoreCase))
            return new DateTimeRange(relativeBaseTime.SubtractDays(1).StartOfDay(), relativeBaseTime.SubtractDays(1).EndOfDay());
        if (String.Equals(value, "tomorrow", StringComparison.OrdinalIgnoreCase))
            return new DateTimeRange(relativeBaseTime.AddDays(1).StartOfDay(), relativeBaseTime.AddDays(1).EndOfDay());

        return null;
    }
}
