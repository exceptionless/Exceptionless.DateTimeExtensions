using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers;

[Priority(80)]
public partial class YearFormatParser : IFormatParser
{
    [GeneratedRegex(@"^\s*(?<year>\d{4})\s*$")]
    private static partial Regex Parser();

    public DateTimeRange? Parse(string content, DateTimeOffset relativeBaseTime)
    {
        var m = Parser().Match(content);
        if (!m.Success)
            return null;

        int year = Int32.Parse(m.Groups["year"].Value);
        var target = relativeBaseTime.ChangeYear(year);
        return new DateTimeRange(target.StartOfYear(), target.EndOfYear());
    }
}
