using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers;
using Xunit;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers {
    public class NamedDayFormatParserTests : FormatParserTestsBase {
        [Theory]
        [MemberData(nameof(Inputs))]
        public void ParseInput(string input, DateTime? start, DateTime? end) {
            ValidateInput(new NamedDayFormatParser(), input, start, end);
        }

        public static IEnumerable<object[]> Inputs {
            get {
                return new[] {
                    new object[] { "yesterday", _now.SubtractDays(1).StartOfDay(), _now.SubtractDays(1).EndOfDay() },
                    new object[] { "today",     _now.StartOfDay(), _now.EndOfDay() },
                    new object[] { "tomorrow",  _now.AddDays(1).StartOfDay(), _now.AddDays(1).EndOfDay() },
                    new object[] { "blah",      null, null },
                    new object[] { "blah blah", null, null }
                };
            }
        }
    }
}
