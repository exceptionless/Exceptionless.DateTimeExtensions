using System;
using Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;
using Foundatio.Logging.Xunit;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers.PartParsers {
    public abstract class PartParserTestsBase : TestWithLoggingBase {
        protected static readonly DateTimeOffset _now;

        protected PartParserTestsBase(ITestOutputHelper output) : base(output) {}

        static PartParserTestsBase() {
            _now = RandomHelper.GetRandomDate(new DateTimeOffset(2014, 11, 1, 0, 0, 0, TimeSpan.FromHours(1)), new DateTimeOffset(2014, 12, 1, 0, 0, 0, TimeSpan.FromHours(1)).SubtractMilliseconds(1));
        }

        public void ValidateInput(IPartParser parser, string input, bool isUpperLimit, DateTimeOffset? expected) {
            _logger.LogInformation("Input: {Input}, Now: {Now}, IsUpperLimit: {IsUpperLimit}, Expected: {Expected}", input, _now, isUpperLimit, expected);
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
