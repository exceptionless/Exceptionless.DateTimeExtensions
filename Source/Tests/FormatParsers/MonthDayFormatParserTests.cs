using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers;
using Xunit;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers {
    public class MonthDayFormatParserTests : FormatParserTestsBase {
        [Theory]
        [MemberData("Inputs")]
        public void ParseInput(string input, DateTime? start, DateTime? end) {
            ValidateInput(new MonthDayFormatParser(), input, start, end);
        }

        public static IEnumerable<object[]> Inputs {
            get {
                return new[] {
                    new object[] { "02-01",     _now.Change(null, 2, 1).StartOfDay(), _now.Change(null, 2, 1).EndOfDay() },
                    new object[] { "11-06",     _now.Change(null, 11, 6).StartOfDay(), _now.Change(null, 11, 6).EndOfDay() },
                    new object[] { "12-24",     _now.Change(null, 12, 24).StartOfDay(), _now.Change(null, 12, 24).EndOfDay() },
                    new object[] { "12-45",     null, null },
                    new object[] { "blah",      null, null },
                    new object[] { "blah blah", null, null }
                };
            }
        }
    }
}
