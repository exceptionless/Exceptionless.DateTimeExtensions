using System.Globalization;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers;

[Priority(30)]
public partial class ExplicitDateFormatParser : IFormatParser
{
    [GeneratedRegex(@"^\s*(?<date>\d{4}-\d{2}-\d{2}(?:T(?:\d{2}\:\d{2}\:\d{2}(?:\.\d{3})?|\d{2}\:\d{2}|\d{2})Z?)?)\s*$")]
    private static partial Regex Parser();

    public DateTimeRange? Parse(string content, DateTimeOffset relativeBaseTime)
    {
        content = content.Trim();
        var m = Parser().Match(content);
        if (!m.Success)
            return null;

        string value = m.Groups["date"].Value;
        if (value.Length == 13)
            value += ":00:00";
        if (value.Length == 16)
            value += ":00";

        // NOTE: AssumeUniversal here because this might parse a date (E.G., 03/22/2023). If no offset is specified, we assume it's UTC.
        if (!DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date))
            return null;

        if (relativeBaseTime.Offset != date.Offset)
            date = date.ChangeOffset(relativeBaseTime.Offset);

        return content.Length switch
        {
            10 => new DateTimeRange(date, date.EndOfDay()),
            13 => new DateTimeRange(date, date.EndOfHour()),
            16 => new DateTimeRange(date, date.EndOfMinute()),
            _ => new DateTimeRange(date, date.EndOfSecond())
        };
    }
}
