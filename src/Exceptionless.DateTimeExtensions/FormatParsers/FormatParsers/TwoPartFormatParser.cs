using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;

namespace Exceptionless.DateTimeExtensions.FormatParsers;

[Priority(25)]
public class TwoPartFormatParser : IFormatParser
{
    private static readonly Regex _beginRegex = new(@"^\s*([\[\{])?\s*", RegexOptions.Compiled);
    private static readonly Regex _delimiterRegex = new(@"\G(?:\s*-\s*|\s+TO\s+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex _endRegex = new(@"\G\s*([\]\}])?\s*$", RegexOptions.Compiled);

    public TwoPartFormatParser()
    {
        Parsers = new List<IPartParser>(DateTimeRange.PartParsers);
    }

    public TwoPartFormatParser(IEnumerable<IPartParser> parsers, bool includeDefaults = false)
    {
        Parsers = new List<IPartParser>(parsers);
        if (includeDefaults)
            Parsers.AddRange(DateTimeRange.PartParsers);
    }

    public List<IPartParser> Parsers { get; private set; }

    public DateTimeRange Parse(string content, DateTimeOffset relativeBaseTime)
    {
        if (String.IsNullOrEmpty(content))
            return null;

        var begin = _beginRegex.Match(content);
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

        // Inclusive min ([): round down (start of period) — ">= start"
        // Exclusive min ({): round up (end of period) — "> end"
        bool minInclusive = openingBracket != '{';

        // Inclusive max (]): round up (end of period) — "<= end"
        // Exclusive max (}): round down (start of period) — "< start"
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
            if (start == null)
                continue;

            index += match.Length;
            break;
        }

        var delimiter = _delimiterRegex.Match(content, index);
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
            if (end == null)
                continue;

            index += match.Length;
            break;
        }

        var endMatch = _endRegex.Match(content, index);
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
    /// Both Elasticsearch bracket types can be mixed: [ with ], [ with }, { with ], { with }.
    /// </summary>
    private static bool IsValidBracketPair(char? opening, char? closing)
    {
        if (opening == null && closing == null)
            return true;

        if (opening == null || closing == null)
            return false;

        bool validOpening = opening is '[' or '{';
        bool validClosing = closing is ']' or '}';
        return validOpening && validClosing;
    }
}
