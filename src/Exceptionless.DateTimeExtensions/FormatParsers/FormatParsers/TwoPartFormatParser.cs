using System.Text.RegularExpressions;
using Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;

namespace Exceptionless.DateTimeExtensions.FormatParsers;

[Priority(25)]
public partial class TwoPartFormatParser : IFormatParser
{
    [GeneratedRegex(@"^\s*([\[\{])?\s*")]
    private static partial Regex BeginRegex();

    [GeneratedRegex(@"\G(?:\s*-\s*|\s+TO\s+)", RegexOptions.IgnoreCase)]
    private static partial Regex DelimiterRegex();

    [GeneratedRegex(@"\G\s*([\]\}])?\s*$")]
    private static partial Regex EndRegex();

    public TwoPartFormatParser()
    {
        Parsers = new List<IPartParser>(DateTimeRange.PartParsers).AsReadOnly();
    }

    public TwoPartFormatParser(IEnumerable<IPartParser> parsers, bool includeDefaults = false)
    {
        var list = new List<IPartParser>(parsers);
        if (includeDefaults)
            list.AddRange(DateTimeRange.PartParsers);
        Parsers = list.AsReadOnly();
    }

    public IReadOnlyList<IPartParser> Parsers { get; private set; }

    public DateTimeRange? Parse(string content, DateTimeOffset relativeBaseTime)
    {
        if (String.IsNullOrEmpty(content))
            return null;

        var begin = BeginRegex().Match(content);
        if (!begin.Success)
            return null;

        string openingValue = begin.Groups[1].Value;
        char? openingBracket = openingValue.Length > 0 ? openingValue[0] : (char?)null;

        // Scan backwards from end of string to find closing bracket character.
        // This is cheaper than a regex and lets us determine max inclusivity upfront.
        char? closingBracket = null;
        for (int pos = content.Length - 1; pos >= 0; pos--)
        {
            char ch = content[pos];
            if (ch is ']' or '}')
            {
                closingBracket = ch;
                break;
            }

            if (!Char.IsWhiteSpace(ch))
                break;
        }

        if (!IsValidBracketPair(openingBracket, closingBracket))
            return null;

        // Determine isUpperLimit from bracket inclusivity (per Elasticsearch date math rounding spec):
        // Inclusive min ([): round down (start of period) — ">= start"
        // Exclusive min ({): round up (end of period) — "> end"
        // Inclusive max (]): round up (end of period) — "<= end"
        // Exclusive max (}): round down (start of period) — "< start"
        bool minInclusive = openingBracket != '{';
        bool maxInclusive = closingBracket != '}';

        int index = begin.Length;
        DateTimeOffset? start = null;
        foreach (var parser in Parsers)
        {
            var match = parser.Regex.Match(content, index);
            if (!match.Success)
                continue;

            // Wildcard parsers use isUpperLimit for position (min/max), not rounding.
            // For non-wildcard parsers, bracket inclusivity determines rounding direction.
            bool isUpperLimit = parser is not WildcardPartParser && !minInclusive;
            start = parser.Parse(match, relativeBaseTime, isUpperLimit);
            if (start is null)
                continue;

            index += match.Length;
            break;
        }

        var delimiter = DelimiterRegex().Match(content, index);
        if (!delimiter.Success)
            return null;

        index += delimiter.Length;

        DateTimeOffset? end = null;
        foreach (var parser in Parsers)
        {
            var match = parser.Regex.Match(content, index);
            if (!match.Success)
                continue;

            bool isUpperLimit = parser is WildcardPartParser || maxInclusive;
            end = parser.Parse(match, relativeBaseTime, isUpperLimit);
            if (end is null)
                continue;

            index += match.Length;
            break;
        }

        var endMatch = EndRegex().Match(content, index);
        if (!endMatch.Success)
            return null;

        var rangeStart = start ?? DateTimeOffset.MinValue;
        var rangeEnd = end ?? DateTimeOffset.MaxValue;

        // Bracket-aware rounding can produce start > end (e.g., "{now/d TO now/d}" yields
        // end-of-day then start-of-day). Collapse to a single instant rather than letting
        // DateTimeRange reorder the bounds and unintentionally expand the range.
        if (rangeStart > rangeEnd)
            return new DateTimeRange(rangeEnd, rangeEnd);

        return new DateTimeRange(rangeStart, rangeEnd);
    }

    /// <summary>
    /// Validates that opening and closing brackets form a valid pair.
    /// All four Elasticsearch bracket combinations are valid: [/], [/}, {/], {/}.
    /// </summary>
    private static bool IsValidBracketPair(char? opening, char? closing)
    {
        if (opening is null && closing is null)
            return true;

        if (opening is null || closing is null)
            return false;

        bool validOpening = opening is '[' or '{';
        bool validClosing = closing is ']' or '}';
        return validOpening && validClosing;
    }
}
