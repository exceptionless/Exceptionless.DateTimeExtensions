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

        // Capture the opening bracket if present
        string openingBracket = begin.Groups[1].Value;

        index += begin.Length;
        DateTimeOffset? start = null;
        foreach (var parser in Parsers)
        {
            var match = parser.Regex.Match(content, index);
            if (!match.Success)
                continue;

            start = parser.Parse(match, relativeBaseTime, false);
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

            end = parser.Parse(match, relativeBaseTime, true);
            if (end == null)
                continue;

            index += match.Length;
            break;
        }

        var endMatch = _endRegex.Match(content, index);
        if (!endMatch.Success)
            return null;

        // Validate bracket matching
        string closingBracket = endMatch.Groups[1].Value;
        if (!IsValidBracketPair(openingBracket, closingBracket))
            return null;

        return new DateTimeRange(start ?? DateTime.MinValue, end ?? DateTime.MaxValue);
    }

    /// <summary>
    /// Validates that opening and closing brackets are properly matched.
    /// </summary>
    /// <param name="opening">The opening bracket character</param>
    /// <param name="closing">The closing bracket character</param>
    /// <returns>True if brackets are properly matched, false otherwise</returns>
    private static bool IsValidBracketPair(string opening, string closing)
    {
        // Both empty - valid (no brackets)
        if (String.IsNullOrEmpty(opening) && String.IsNullOrEmpty(closing))
            return true;

        // One empty, one not - invalid (unbalanced)
        if (String.IsNullOrEmpty(opening) || String.IsNullOrEmpty(closing))
            return false;

        // Check for proper matching pairs
        return (opening == "[" && closing == "]") ||
               (opening == "{" && closing == "}");
    }
}
