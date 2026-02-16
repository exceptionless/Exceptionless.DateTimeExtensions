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
        int index = 0;
        var begin = _beginRegex.Match(content, index);
        if (!begin.Success)
            return null;

        string openingBracket = begin.Groups[1].Value;

        // Scan backwards from end of string to find closing bracket character.
        // This is cheaper than a regex and lets us determine max inclusivity upfront.
        string closingBracket = "";
        for (int i = content.Length - 1; i >= 0; i--)
        {
            char c = content[i];
            if (c == ']' || c == '}')
            {
                closingBracket = c.ToString();
                break;
            }

            if (!Char.IsWhiteSpace(c))
                break;
        }

        if (!IsValidBracketPair(openingBracket, closingBracket))
            return null;

        // Inclusive min ([): round down (start of period) — ">= start"
        // Exclusive min ({): round up (end of period) — "> end"
        bool minInclusive = !String.Equals(openingBracket, "{");

        // Inclusive max (]): round up (end of period) — "<= end"
        // Exclusive max (}): round down (start of period) — "< start"
        bool maxInclusive = !String.Equals(closingBracket, "}");

        index += begin.Length;
        DateTimeOffset? start = null;
        foreach (var parser in Parsers)
        {
            var match = parser.Regex.Match(content, index);
            if (!match.Success)
                continue;

            // Wildcard parsers use isUpperLimit for position (min/max), not rounding.
            // For non-wildcard parsers, bracket inclusivity determines rounding direction.
            bool isUpperLimit = parser is WildcardPartParser ? false : !minInclusive;
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

            bool isUpperLimit = parser is WildcardPartParser ? true : maxInclusive;
            end = parser.Parse(match, relativeBaseTime, isUpperLimit);
            if (end == null)
                continue;

            index += match.Length;
            break;
        }

        var endMatch = _endRegex.Match(content, index);
        if (!endMatch.Success)
            return null;

        return new DateTimeRange(start ?? DateTime.MinValue, end ?? DateTime.MaxValue);
    }

    /// <summary>
    /// Validates that opening and closing brackets form a valid pair.
    /// Both Elasticsearch bracket types can be mixed: [ with ], [ with }, { with ], { with }.
    /// </summary>
    /// <param name="opening">The opening bracket character</param>
    /// <param name="closing">The closing bracket character</param>
    /// <returns>True if brackets are properly paired, false otherwise</returns>
    private static bool IsValidBracketPair(string opening, string closing)
    {
        // Both empty - valid (no brackets)
        if (String.IsNullOrEmpty(opening) && String.IsNullOrEmpty(closing))
            return true;

        // One empty, one not - invalid (unbalanced)
        if (String.IsNullOrEmpty(opening) || String.IsNullOrEmpty(closing))
            return false;

        // Any valid opening bracket ([, {) can pair with any valid closing bracket (], })
        bool validOpening = String.Equals(opening, "[") || String.Equals(opening, "{");
        bool validClosing = String.Equals(closing, "]") || String.Equals(closing, "}");
        return validOpening && validClosing;
    }
}
