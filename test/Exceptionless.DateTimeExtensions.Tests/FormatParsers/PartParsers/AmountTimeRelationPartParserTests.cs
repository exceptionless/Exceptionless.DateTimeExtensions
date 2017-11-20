using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;
using Xunit;
using Xunit.Abstractions;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers.PartParsers {
    public class AmountTimeRelationPartParserTests : PartParserTestsBase {
        public AmountTimeRelationPartParserTests(ITestOutputHelper output) : base(output) { }

        [Theory]
        [MemberData(nameof(Inputs))]
        public void ParseInput(string input, bool isUpperLimit, DateTimeOffset? expected) {
            ValidateInput(new AmountTimeRelationPartParser(), input, isUpperLimit, expected);
        }

        public static IEnumerable<object[]> Inputs {
            get {
                return new[] {
                    new object[] { "1 minute ago",      false, _now.SubtractMinutes(1).StartOfMinute() },
                    new object[] { "1 minute ago",      true,  _now.SubtractMinutes(1).EndOfMinute() },
                    new object[] { "1 minute from now", false, _now.AddMinutes(1).StartOfMinute() },
                    new object[] { "1 minute from now", true,  _now.AddMinutes(1).EndOfMinute() },
                    new object[] { "22 hours ago",      false, _now.SubtractHours(22).StartOfHour() },
                    new object[] { "22 hours ago",      true,  _now.SubtractHours(22).EndOfHour() },
                    new object[] { "12 days from now",  false, _now.AddDays(12).StartOfDay() },
                    new object[] { "12 days from now",  true,  _now.AddDays(12).EndOfDay() },
                    new object[] { "blah",              false, null },
                    new object[] { "blah blah",         true,  null }
                };
            }
        }
    }
}
