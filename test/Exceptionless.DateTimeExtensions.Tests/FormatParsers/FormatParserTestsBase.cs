using System;
using Exceptionless.DateTimeExtensions.FormatParsers;
using Foundatio.Logging;
using Foundatio.Logging.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers {
    public abstract class FormatParserTestsBase : TestWithLoggingBase {
        protected static readonly DateTime _now;

        protected FormatParserTestsBase(ITestOutputHelper output) : base(output) {}

        static FormatParserTestsBase() {
            _now = RandomHelper.GetRandomDate(new DateTime(2014, 11, 1), new DateTime(2014, 12, 1).SubtractMilliseconds(1));
        }

        public void ValidateInput(IFormatParser parser, string input, DateTime? start, DateTime? end) {
            _logger.Info(String.Format("Now: {0}", _now));
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
