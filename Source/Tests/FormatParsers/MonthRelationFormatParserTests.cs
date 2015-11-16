using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers;
using Xunit;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers {
    public class MonthRelationFormatParserTests : FormatParserTestsBase {
        [Theory]
        [MemberData("Inputs")]
        public void ParseInput(string input, DateTime? start, DateTime? end) {
            ValidateInput(new MonthRelationFormatParser(), input, start, end);
        }

        public static IEnumerable<object[]> Inputs {
            get {
                return new[] {
                    new object[] { "this janUary",  _now.AddYears(1).ChangeMonth(1).StartOfMonth(), _now.AddYears(1).ChangeMonth(1).EndOfMonth() },
                    new object[] { "last jan",      _now.ChangeMonth(1).StartOfMonth(), _now.ChangeMonth(1).EndOfMonth() },
                    new object[] { "next januaRY",  _now.AddYears(1).ChangeMonth(1).StartOfMonth(), _now.AddYears(1).ChangeMonth(1).EndOfMonth() },
                    new object[] { "this november", _now.StartOfMonth(), _now.EndOfMonth() },
                    new object[] { "next november", _now.AddYears(1).StartOfMonth(), _now.AddYears(1).EndOfMonth() },
                    new object[] { "this december", _now.ChangeMonth(12).StartOfMonth(), _now.ChangeMonth(12).EndOfMonth() },
                    new object[] { "last november", _now.SubtractYears(1).StartOfMonth(), _now.SubtractYears(1).EndOfMonth() },
                    new object[] { "blah",          null, null },
                    new object[] { "blah blah",     null, null }
                };
            }
        }
    }
}
