using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers;

[Priority(50)]
public partial class MonthDayFormatParser : IFormatParser
{
    [GeneratedRegex(@"^\s*(?<month>\d{2})-(?<day>\d{2})\s*$")]
    private static partial Regex Parser();

    public DateTimeRange? Parse(string content, DateTimeOffset relativeBaseTime)
    {
        var m = Parser().Match(content);
        if (!m.Success)
            return null;

        int month = Int32.Parse(m.Groups["month"].Value);
        int day = Int32.Parse(m.Groups["day"].Value);
        try
        {
            var target = relativeBaseTime.ChangeMonth(month).ChangeDay(day);
            return new DateTimeRange(target.StartOfDay(), target.EndOfDay());
        }
        catch
        {
            return null;
        }
    }
}
