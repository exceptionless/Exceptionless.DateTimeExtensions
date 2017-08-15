using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;
using Xunit;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers.PartParsers {
    public class ExplicitDatePartParserTests : PartParserTestsBase {
        [Theory]
        [MemberData(nameof(Inputs))]
        public void ParseInput(string input, bool isUpperLimit, DateTimeOffset? expected) {
            ValidateInput(new ExplicitDatePartParser(), input, isUpperLimit, expected);
        }

        public static IEnumerable<object[]> Inputs {
            get {
                return new[] {
                    new object[] { "2014-02-01",          false, _now.Change(null, 2, 1).StartOfDay() },
                    new object[] { "2014-02-01",          true,  _now.Change(null, 2, 1).EndOfDay() },
                    new object[] { "2014-02-01T05",       false, _now.Change(null, 2, 1, 5).StartOfHour() },
                    new object[] { "2014-02-01T05",       true,  _now.Change(null, 2, 1, 5).EndOfHour() },
                    new object[] { "2014-02-01T05:30",    false, _now.Change(null, 2, 1, 5, 30).StartOfMinute() },
                    new object[] { "2014-02-01T05:30",    true,  _now.Change(null, 2, 1, 5, 30).EndOfMinute() },
                    new object[] { "2014-02-01T05:30:20", false, _now.Change(null, 2, 1, 5, 30, 20).StartOfSecond() },
                    new object[] { "2014-02-01T05:30:20", true,  _now.Change(null, 2, 1, 5, 30, 20).StartOfSecond() },
                    new object[] { "2014-11-06",          false, _now.Change(null, 11, 6).StartOfDay() },
                    new object[] { "2014-11-06",          true,  _now.Change(null, 11, 6).EndOfDay() },
                    new object[] { "2014-12-24",          false, _now.Change(null, 12, 24).StartOfDay() },
                    new object[] { "2014-12-24",          true,  _now.Change(null, 12, 24).EndOfDay() },
                    new object[] { "2014-12-45",          true,  null },
                    new object[] { "blah",                false, null },
                    new object[] { "blah blah",           true,  null }
                };
            }
        }
    }
}
