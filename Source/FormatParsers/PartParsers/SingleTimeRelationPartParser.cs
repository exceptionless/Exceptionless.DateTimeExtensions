using System;
using System.Text.RegularExpressions;

namespace Exceptionless.DateTimeExtensions.FormatParsers.PartParsers {
    [Priority(80)]
    public class SingleTimeRelationPartParser : AmountTimeRelationPartParser {
        private static readonly Regex _parser = new Regex(String.Format(@"\G(?:a|an)\s+(?<time>{0})\s+(?<relation>ago|from now)", Helper.SingularTimeNames), RegexOptions.IgnoreCase);
        public override Regex Regex { get { return _parser; } }

        public override DateTime? Parse(Match match, DateTime now, bool isUpperLimit) {
            return FromRelationAmountTime(
                    match.Groups["relation"].Value,
                    1,
                    match.Groups["time"].Value,
                    now,
                    isUpperLimit);
        }
    }
}
