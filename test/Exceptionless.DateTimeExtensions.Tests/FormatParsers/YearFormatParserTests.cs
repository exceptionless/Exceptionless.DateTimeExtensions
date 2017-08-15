using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers;
using Xunit;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers {
    public class YearFormatParserTests : FormatParserTestsBase {
        [Theory]
        [MemberData(nameof(Inputs))]
        public void ParseInput(string input, DateTime? start, DateTime? end) {
            ValidateInput(new YearFormatParser(), input, start, end);
        }

        public static IEnumerable<object[]> Inputs {
            get {
                return new[] {
                    new object[] { "2013",      _now.ChangeYear(2013).StartOfYear(), _now.ChangeYear(2013).EndOfYear() },
                    new object[] { "2012",      _now.ChangeYear(2012).StartOfYear(), _now.ChangeYear(2012).EndOfYear() },
                    new object[] { "blah",      null, null },
                    new object[] { "blah blah", null, null }
                };
            }
        }
    }
}
