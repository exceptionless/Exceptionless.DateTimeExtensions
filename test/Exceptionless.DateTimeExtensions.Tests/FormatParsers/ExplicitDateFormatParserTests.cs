using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers;
using Xunit;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers {
    public class ExplicitDateFormatParserTests : FormatParserTestsBase {
        [Theory]
        [MemberData(nameof(Inputs))]
        public void ParseInput(string input, DateTime? start, DateTime? end) {
            ValidateInput(new ExplicitDateFormatParser(), input, start, end);
        }

        public static IEnumerable<object[]> Inputs {
            get {
                return new[] {
                    new object[] { "2014-02-01",          _now.Change(null, 2, 1).StartOfDay(), _now.Change(null, 2, 1).EndOfDay() },
                    new object[] { "2014-02-01T05",       _now.Change(null, 2, 1, 5).StartOfHour(), _now.Change(null, 2, 1, 5).EndOfHour() },
                    new object[] { "2014-02-01T05:30",    _now.Change(null, 2, 1, 5, 30).StartOfMinute(), _now.Change(null, 2, 1, 5, 30).EndOfMinute() },
                    new object[] { "2014-02-01T05:30:20", _now.Change(null, 2, 1, 5, 30, 20).StartOfSecond(), _now.Change(null, 2, 1, 5, 30, 20).EndOfSecond() },
                    new object[] { "2014-11-06",          _now.Change(null, 11, 6).StartOfDay(), _now.Change(null, 11, 6).EndOfDay() },
                    new object[] { "2014-12-24",          _now.Change(null, 12, 24).StartOfDay(), _now.Change(null, 12, 24).EndOfDay() },
                    new object[] { "2014-12-45",          null, null },
                    new object[] { "blah",                null, null },
                    new object[] { "blah blah",           null, null }
                };
            }
        }
    }
}
