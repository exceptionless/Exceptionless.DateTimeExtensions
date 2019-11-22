using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;
using Xunit;
using Xunit.Abstractions;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers.PartParsers {
    public class MonthRelationPartParserTests : PartParserTestsBase {
        public MonthRelationPartParserTests(ITestOutputHelper output) : base(output) { }

        [Theory]
        [MemberData(nameof(Inputs))]
        public void ParseInput(string input, bool isUpperLimit, DateTimeOffset? expected) {
            ValidateInput(new MonthRelationPartParser(), input, isUpperLimit, expected);
        }

        public static IEnumerable<object[]> Inputs {
            get {
                return new[] {
                    new object[] { "this jan",      false, _now.AddYears(1).ChangeMonth(1).StartOfMonth() },
                    new object[] { "this janUary",  true,  _now.AddYears(1).ChangeMonth(1).EndOfMonth() },
                    new object[] { "last jan",      false, _now.ChangeMonth(1).StartOfMonth() },
                    new object[] { "last jan",      true,  _now.ChangeMonth(1).EndOfMonth() },
                    new object[] { "next januaRY",  false, _now.AddYears(1).ChangeMonth(1).StartOfMonth() },
                    new object[] { "next jan",      true,  _now.AddYears(1).ChangeMonth(1).EndOfMonth() },
                    new object[] { "this november", false, _now.StartOfMonth() },
                    new object[] { "this november", true,  _now.EndOfMonth() },
                    new object[] { "next november", false, _now.AddYears(1).StartOfMonth() },
                    new object[] { "next november", true,  _now.AddYears(1).EndOfMonth() },
                    new object[] { "this december", true,  _now.ChangeMonth(12).EndOfMonth() },
                    new object[] { "last november", false, _now.SubtractYears(1).StartOfMonth() },
                    new object[] { "blah",          false, null },
                    new object[] { "blah blah",     true,  null }
                };
            }
        }
    }
}
