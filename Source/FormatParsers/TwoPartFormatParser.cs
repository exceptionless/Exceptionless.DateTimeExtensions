using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;

namespace Exceptionless.DateTimeExtensions.FormatParsers {
    [Priority(25)]
    public class TwoPartFormatParser : IFormatParser {
        private static readonly Regex _beginRegex = new Regex(@"^\s*");
        private static readonly Regex _delimiterRegex = new Regex(@"\G(?:\s*-\s*|\s+TO\s+)", RegexOptions.IgnoreCase);
        private static readonly Regex _endRegex = new Regex(@"\G\s*$");

        public TwoPartFormatParser() {
            Parsers = new List<IPartParser>(DateTimeRange.PartParsers);
        }

        public TwoPartFormatParser(IEnumerable<IPartParser> parsers, bool includeDefaults = false) {
            Parsers = new List<IPartParser>(parsers);
            if (includeDefaults)
                Parsers.AddRange(DateTimeRange.PartParsers);
        }

        public List<IPartParser> Parsers { get; private set; } 

        public DateTimeRange Parse(string content, DateTime now) {
            int index = 0;
            var begin = _beginRegex.Match(content, index);
            if (!begin.Success)
                return null;
            index += begin.Length;

            DateTime? start = null;
            foreach (var parser in Parsers) {
                Match match = parser.Regex.Match(content, index);
                if (!match.Success)
                    continue;

                start = parser.Parse(match, now, false);
                if (start == null)
                    continue;

                index += match.Length;
                break;
            }

            var delimiter = _delimiterRegex.Match(content, index);
            if (!delimiter.Success)
                return null;

            index += delimiter.Length;

            DateTime? end = null;
            foreach (var parser in Parsers) {
                Match match = parser.Regex.Match(content, index);
                if (!match.Success)
                    continue;

                end = parser.Parse(match, now, true);
                if (end == null)
                    continue;

                index += match.Length;
                break;
            }

            if (!_endRegex.IsMatch(content, index))
                return null;

            if (start == null)
                start = DateTime.MinValue;
            if (end == null)
                end = DateTime.MaxValue;

            return new DateTimeRange(start.Value, end.Value);
        }
    }
}
