using System;
using System.Diagnostics;
using Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;
using Xunit;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers.PartParsers {
    public abstract class PartParserTestsBase {
        protected static DateTime _now;

        static PartParserTestsBase() {
            _now = RandomHelper.GetRandomDate(new DateTime(2014, 11, 1), new DateTime(2014, 12, 1).SubtractMilliseconds(1));
        }

        public void ValidateInput(IPartParser parser, string input, bool isUpperLimit, DateTime? expected) {
            Trace.WriteLine(String.Format("Now: {0}", _now));
            var match = parser.Regex.Match(input);
            if (!match.Success) {
                Assert.Null(expected);
                return;
            }

            var result = parser.Parse(match, _now, isUpperLimit);
            if (expected == null) {
                Assert.Null(result);
            } else {
                Assert.Equal(expected, result.Value);
            }
        }
    }
}
