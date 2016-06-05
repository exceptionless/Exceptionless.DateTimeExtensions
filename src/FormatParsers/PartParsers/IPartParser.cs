using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers {
    public interface IPartParser {
        Regex Regex { get; }
        DateTime? Parse(Match match, DateTime now, bool isUpperLimit);
    }
}