using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;

/// <summary>
/// Parses Elasticsearch date math expressions using the DateMath utility class.
/// This is a thin wrapper that adapts the DateMath utility for use within the format parser framework.
///
/// Supports: now, explicit dates with ||, operations (+, -, /), and time units (y, M, w, d, h, H, m, s).
/// Examples: now+1h, now-1d/d, 2001.02.01||+1M/d, 2025-01-01T01:25:35Z||+3d/d
///
/// For more details about date math functionality, see <see cref="DateMath"/>.
/// </summary>
[Priority(5)]
public class DateMathPartParser : IPartParser
{
    public Regex Regex => DateMath.Parser;

    public DateTimeOffset? Parse(Match match, DateTimeOffset relativeBaseTime, bool isUpperLimit)
    {
        // Since we already have a successful regex match, use the efficient TryParseFromMatch method
        // to avoid redundant regex matching and validation
        return DateMath.TryParseFromMatch(match, relativeBaseTime, isUpperLimit, out DateTimeOffset result)
            ? result
            : null;
    }
}
