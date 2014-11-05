using System;
using System.Diagnostics;
using Exceptionless.DateTimeExtensions.FormatParsers;
using Xunit;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers {
    public abstract class FormatParserTestsBase {
        protected static DateTime _now = DateTime.Now;

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
