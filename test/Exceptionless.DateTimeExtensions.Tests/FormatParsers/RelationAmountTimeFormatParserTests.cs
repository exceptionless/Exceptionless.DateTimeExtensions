using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers;
using Xunit;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers {
    public class RelationAmountTimeFormatParserTests : FormatParserTestsBase {
        [Theory]
        [MemberData(nameof(Inputs))]
        public void ParseInput(string input, DateTime? start, DateTime? end) {
            ValidateInput(new RelationAmountTimeFormatParser(), input, start, end);
        }

        public static IEnumerable<object[]> Inputs {
            get {
                return new[] {
                    new object[] { "last 5 minutes",   _now.StartOfMinute().SubtractMinutes(5), _now },
                    new object[] { "last 30 days",     _now.StartOfDay().SubtractDays(30),  _now },
                    new object[] { "next 5 weeks",     _now, _now.AddWeeks(5).EndOfDay() },
                    new object[] { "this 3 hours",     _now, _now.AddHours(3).EndOfHour() },
                    new object[] { "previous 3 years", _now.SubtractYears(3).StartOfDay(), _now },
                    new object[] { "blah",             null, null },
                    new object[] { "blah blah",        null,  null }
                };
            }
        }
    }
}
