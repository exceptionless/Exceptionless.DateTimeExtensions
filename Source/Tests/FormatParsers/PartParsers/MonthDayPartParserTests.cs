using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;
using Xunit.Extensions;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers.PartParsers {
    public class MonthDayPartParserTests : PartParserTestsBase {
        [Theory]
        [PropertyData("Inputs")]
        public void ParseInput(string input, bool isUpperLimit, DateTime? expected) {
            ValidateInput(new MonthDayPartParser(), input, isUpperLimit, expected);
        }

        public static IEnumerable<object[]> Inputs {
            get {
                return new[] {
                    new object[] { "02-01",     false, _now.ChangeMonth(2).ChangeDay(1).StartOfDay() },
                    new object[] { "02-01",     true,  _now.ChangeMonth(2).ChangeDay(1).EndOfDay() },
                    new object[] { "11-06",     false, _now.ChangeMonth(11).ChangeDay(6).StartOfDay() },
                    new object[] { "11-06",     true,  _now.ChangeMonth(11).ChangeDay(6).EndOfDay() },
                    new object[] { "12-24",     false, _now.ChangeMonth(12).ChangeDay(24).StartOfDay() },
                    new object[] { "12-24",     true,  _now.ChangeMonth(12).ChangeDay(24).EndOfDay() },
                    new object[] { "12-45",     true,  null },
                    new object[] { "blah",      false, null },
                    new object[] { "blah blah", true,  null }
                };
            }
        }
    }
}
