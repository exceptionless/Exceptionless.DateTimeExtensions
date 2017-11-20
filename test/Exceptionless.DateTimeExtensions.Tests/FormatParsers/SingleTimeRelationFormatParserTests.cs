using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers;
using Xunit;
using Xunit.Abstractions;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers {
    public class SingleTimeRelationFormatParserTests : FormatParserTestsBase {
        public SingleTimeRelationFormatParserTests(ITestOutputHelper output) : base(output) { }

        [Theory]
        [MemberData(nameof(Inputs))]
        public void ParseInput(string input, DateTime? start, DateTime? end) {
            ValidateInput(new SingleTimeRelationFormatParser(), input, start, end);
        }

        public static IEnumerable<object[]> Inputs {
            get {
                return new[] {
                    new object[] { "past minute",   _now.SubtractMinutes(1).StartOfMinute(), _now },
                    new object[] { "next minute",   _now, _now.AddMinutes(1).EndOfMinute() },
                    new object[] { "last hour",     _now.SubtractHours(1).StartOfHour(), _now },
                    new object[] { "next hour",     _now, _now.AddHours(1).EndOfHour() },
                    new object[] { "previous day",  _now.SubtractDays(1).StartOfDay(), _now },
                    new object[] { "next day",      _now, _now.AddDays(1).EndOfDay() },
                    new object[] { "next month",    _now, _now.AddMonths(1).EndOfDay() },
                    new object[] { "last month",    _now.SubtractMonths(1).StartOfDay(), _now },
                    new object[] { "previous week", _now.SubtractWeeks(1).StartOfDay(), _now },
                    new object[] { "next week",     _now, _now.AddWeeks(1).EndOfDay() },
                    new object[] { "past year",     _now.SubtractYears(1).StartOfDay(), _now },
                    new object[] { "next year",     _now, _now.AddYears(1).EndOfDay() },
                    new object[] { "blah",          null, null },
                    new object[] { "blah blah",     null, null }
                };
            }
        }
    }
}
