using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;

[Priority(10)]
public class NamedDayPartParser : IPartParser
{
    private static readonly Regex _parser = new(@"\G(?<name>now|today|yesterday|tomorrow)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public Regex Regex => _parser;

    public DateTimeOffset? Parse(Match match, DateTimeOffset relativeBaseTime, bool isUpperLimit)
    {
        string value = match.Groups["name"].Value.ToLower();
        if (value == "now")
            return relativeBaseTime;
        if (value == "today")
            return isUpperLimit ? relativeBaseTime.EndOfDay() : relativeBaseTime.StartOfDay();
        if (value == "yesterday")
            return isUpperLimit ? relativeBaseTime.SubtractDays(1).EndOfDay() : relativeBaseTime.SubtractDays(1).StartOfDay();
        if (value == "tomorrow")
            return isUpperLimit ? relativeBaseTime.AddDays(1).EndOfDay() : relativeBaseTime.AddDays(1).StartOfDay();

        return null;
    }
}
