using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;

public interface IPartParser
{
    Regex Regex { get; }
    DateTimeOffset? Parse(Match match, DateTimeOffset relativeBaseTime, bool isUpperLimit);
}
