using System;
using System.Diagnostics;
using Exceptionless.DateTimeExtensions.FormatParsers;
using Xunit;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers {
    public abstract class FormatParserTestsBase {
        protected static DateTime _now;

        static FormatParserTestsBase() {
            _now = RandomHelper.GetRandomDate(new DateTime(2014, 11, 1), new DateTime(2014, 12, 1).SubtractMilliseconds(1));
        }

        public void ValidateInput(IFormatParser parser, string input, DateTime? start, DateTime? end) {
            Trace.WriteLine(String.Format("Now: {0}", _now));
            var range = parser.Parse(input, _now);
            if (range == null) {
                Assert.Null(start);
                Assert.Null(end);
                return;
            }

            Assert.Equal(start, range.Start);
            Assert.Equal(end, range.End);
        }
    }
}
