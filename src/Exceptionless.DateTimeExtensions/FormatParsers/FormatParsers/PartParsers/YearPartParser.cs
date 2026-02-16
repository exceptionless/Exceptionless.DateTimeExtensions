using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;

[Priority(70)]
public partial class YearPartParser : IPartParser
{
    public Regex Regex => Parser();

    public DateTimeOffset? Parse(Match match, DateTimeOffset relativeBaseTime, bool isUpperLimit)
    {
        int year = Int32.Parse(match.Groups["year"].Value);
        return isUpperLimit ? relativeBaseTime.ChangeYear(year).EndOfYear() : relativeBaseTime.ChangeYear(year).StartOfYear();
    }

    [GeneratedRegex(@"\G(?<year>\d{4})")]
    private static partial Regex Parser();
}
