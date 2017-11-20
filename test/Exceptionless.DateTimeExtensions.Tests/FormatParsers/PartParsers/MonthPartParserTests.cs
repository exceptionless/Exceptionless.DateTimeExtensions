using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;
using Xunit;
using Xunit.Abstractions;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers.PartParsers {
    public class MonthPartParserTests : PartParserTestsBase {
        public MonthPartParserTests(ITestOutputHelper output) : base(output) { }

        [Theory]
        [MemberData(nameof(Inputs))]
        public void ParseInput(string input, bool isUpperLimit, DateTimeOffset? expected) {
            ValidateInput(new MonthPartParser(), input, isUpperLimit, expected);
        }

        public static IEnumerable<object[]> Inputs {
            get {
                return new[] {
                    new object[] { "jan",       false, _now.ChangeMonth(1).StartOfMonth() },
                    new object[] { "jan",       true,  _now.ChangeMonth(1).EndOfMonth() },
                    new object[] { "nov",       false, _now.StartOfMonth() },
                    new object[] { "nov",       true,  _now.EndOfMonth() },
                    new object[] { "decemBer",  false, _now.ChangeMonth(12).StartOfMonth() },
                    new object[] { "dec",       true,  _now.ChangeMonth(12).EndOfMonth() },
                    new object[] { "blah",      false, null },
                    new object[] { "blah blah", true,  null }
                };
            }
        }
    }
}
