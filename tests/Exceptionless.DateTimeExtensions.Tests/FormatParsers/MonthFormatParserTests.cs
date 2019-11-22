using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers;
using Xunit;
using Xunit.Abstractions;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers {
    public class MonthFormatParserTests : FormatParserTestsBase {
        public MonthFormatParserTests(ITestOutputHelper output) : base(output) { }

        [Theory]
        [MemberData(nameof(Inputs))]
        public void ParseInput(string input, DateTime? start, DateTime? end) {
            ValidateInput(new MonthFormatParser(), input, start, end);
        }

        public static IEnumerable<object[]> Inputs {
            get {
                return new[] {
                    new object[] { "jan",       _now.ChangeMonth(1).StartOfMonth(), _now.ChangeMonth(1).EndOfMonth() },
                    new object[] { "nov",       _now.StartOfMonth(), _now.EndOfMonth() },
                    new object[] { "decemBer",  _now.ChangeMonth(12).StartOfMonth(), _now.ChangeMonth(12).EndOfMonth() },
                    new object[] { "blah",      null, null },
                    new object[] { "blah blah", null, null }
                };
            }
        }
    }
}
