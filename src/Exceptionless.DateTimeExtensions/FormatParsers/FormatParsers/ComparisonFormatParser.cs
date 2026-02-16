using System.Text.RegularExpressions;
using Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;

namespace Exceptionless.DateTimeExtensions.FormatParsers;

/// <summary>
/// Parses short-form comparison operators (>, >=, &lt;, &lt;=) as syntactic sugar for
/// ranges with one open end. The operator determines boundary inclusivity, which
/// controls rounding direction for date math expressions.
///
/// Operator mapping (per Elasticsearch range query semantics):
///   >value  → exclusive lower bound → isUpperLimit = true  → rounds to end of period
///   >=value → inclusive lower bound → isUpperLimit = false → rounds to start of period
///   &lt;value  → exclusive upper bound → isUpperLimit = false → rounds to start of period
///   &lt;=value → inclusive upper bound → isUpperLimit = true  → rounds to end of period
///
/// Examples:
///   >now/d  → range from end of today to MaxValue
///   >=now/d → range from start of today to MaxValue
///   &lt;now/d  → range from MinValue to start of today
///   &lt;=now/d → range from MinValue to end of today
/// </summary>
[Priority(24)]
public partial class ComparisonFormatParser : IFormatParser
{
    [GeneratedRegex(@"^\s*(>=?|<=?)\s*")]
    private static partial Regex OperatorRegex();

    public ComparisonFormatParser()
    {
        Parsers = new List<IPartParser>(DateTimeRange.PartParsers).AsReadOnly();
    }

    public IReadOnlyList<IPartParser> Parsers { get; private set; }

    public DateTimeRange? Parse(string content, DateTimeOffset relativeBaseTime)
    {
        var opMatch = OperatorRegex().Match(content);
        if (!opMatch.Success)
            return null;

        string op = opMatch.Groups[1].Value;
        int index = opMatch.Length;

        // Must have an expression after the operator
        if (index >= content.Length || content.AsSpan(index).Trim().IsEmpty)
            return null;

        // Determine inclusivity from operator
        bool isLowerBound = op[0] == '>';
        bool isInclusive = op.Length == 2; // >= or <=

        // isUpperLimit follows the boundary inclusivity rule:
        // For lower bounds: isUpperLimit = !inclusive (gt rounds up, gte rounds down)
        // For upper bounds: isUpperLimit = inclusive (lte rounds up, lt rounds down)
        bool isUpperLimit = isLowerBound ? !isInclusive : isInclusive;

        DateTimeOffset? value = null;
        foreach (var parser in Parsers)
        {
            if (parser is WildcardPartParser)
                continue;

            var match = parser.Regex.Match(content, index);
            if (!match.Success)
                continue;

            value = parser.Parse(match, relativeBaseTime, isUpperLimit);
            if (value is null)
                continue;

            index += match.Length;
            break;
        }

        if (value is null)
            return null;

        // Verify entire input was consumed (only trailing whitespace allowed)
        if (index < content.Length && !content.AsSpan(index).Trim().IsEmpty)
            return null;

        return isLowerBound
            ? new DateTimeRange(value.Value, DateTimeOffset.MaxValue)
            : new DateTimeRange(DateTimeOffset.MinValue, value.Value);
    }
}
