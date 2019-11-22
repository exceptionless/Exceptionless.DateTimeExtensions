using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;
using Xunit;
using Xunit.Abstractions;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers.PartParsers {
    public class SingleTimeRelationPartParserTests : PartParserTestsBase {
        public SingleTimeRelationPartParserTests(ITestOutputHelper output) : base(output) { }

        [Theory]
        [MemberData(nameof(Inputs))]
        public void ParseInput(string input, bool isUpperLimit, DateTimeOffset? expected) {
            ValidateInput(new SingleTimeRelationPartParser(), input, isUpperLimit, expected);
        }

        public static IEnumerable<object[]> Inputs {
            get {
                return new[] {
                    new object[] { "a minute ago",      false, _now.SubtractMinutes(1).StartOfMinute() },
                    new object[] { "a minute ago",      true,  _now.SubtractMinutes(1).EndOfMinute() },
                    new object[] { "a minute from now", false, _now.AddMinutes(1).StartOfMinute() },
                    new object[] { "a minute from now", true,  _now.AddMinutes(1).EndOfMinute() },
                    new object[] { "an hour ago",       false, _now.SubtractHours(1).StartOfHour() },
                    new object[] { "an hour ago",       true,  _now.SubtractHours(1).EndOfHour() },
                    new object[] { "an hour from now",  false, _now.AddHours(1).StartOfHour() },
                    new object[] { "an hour from now",  true,  _now.AddHours(1).EndOfHour() },
                    new object[] { "a day ago",         false, _now.SubtractDays(1).StartOfDay() },
                    new object[] { "a day ago",         true,  _now.SubtractDays(1).EndOfDay() },
                    new object[] { "a day from now",    false, _now.AddDays(1).StartOfDay() },
                    new object[] { "a day from now",    true,  _now.AddDays(1).EndOfDay() },
                    new object[] { "a month ago",       false, _now.SubtractMonths(1).StartOfDay() },
                    new object[] { "a month ago",       true,  _now.SubtractMonths(1).EndOfDay() },
                    new object[] { "a month from now",  false, _now.AddMonths(1).StartOfDay() },
                    new object[] { "a month from now",  true,  _now.AddMonths(1).EndOfDay() },
                    new object[] { "a week ago",        false, _now.SubtractWeeks(1).StartOfDay() },
                    new object[] { "a week ago",        true,  _now.SubtractWeeks(1).EndOfDay() },
                    new object[] { "a week from now",   false, _now.AddWeeks(1).StartOfDay() },
                    new object[] { "a week from now",   true,  _now.AddWeeks(1).EndOfDay() },
                    new object[] { "a year ago",        false, _now.SubtractYears(1).StartOfDay() },
                    new object[] { "a year ago",        true,  _now.SubtractYears(1).EndOfDay() },
                    new object[] { "a year from now",   false, _now.AddYears(1).StartOfDay() },
                    new object[] { "a year from now",   true,  _now.AddYears(1).EndOfDay() },
                    new object[] { "blah",              false, null },
                    new object[] { "blah blah",         true,  null }
                };
            }
        }
    }
}
