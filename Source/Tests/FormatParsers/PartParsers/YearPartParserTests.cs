using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;
using Xunit.Extensions;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers.PartParsers {
    public class YearPartParserTests : PartParserTestsBase {
        [Theory]
        [PropertyData("Inputs")]
        public void ParseInput(string input, bool isUpperLimit, DateTime? expected) {
            ValidateInput(new YearPartParser(), input, isUpperLimit, expected);
        }

        public static IEnumerable<object[]> Inputs {
            get {
                return new[] {
                    new object[] { "2013",      false, _now.ChangeYear(2013).StartOfYear() },
                    new object[] { "2013",      true,  _now.ChangeYear(2013).EndOfYear() },
                    new object[] { "2012",      false, _now.ChangeYear(2012).StartOfYear() },
                    new object[] { "2012",      true,  _now.ChangeYear(2012).EndOfYear() },
                    new object[] { "blah",      false, null },
                    new object[] { "blah blah", true,  null }
                };
            }
        }
    }
}
