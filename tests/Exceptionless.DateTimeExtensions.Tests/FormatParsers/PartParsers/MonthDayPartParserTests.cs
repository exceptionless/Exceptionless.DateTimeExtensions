using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;
using Xunit;
using Xunit.Abstractions;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers.PartParsers;

public class MonthDayPartParserTests : PartParserTestsBase
{
    public MonthDayPartParserTests(ITestOutputHelper output) : base(output) { }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void ParseInput(string input, bool isUpperLimit, DateTimeOffset? expected)
    {
        ValidateInput(new MonthDayPartParser(), input, isUpperLimit, expected);
    }

    public static IEnumerable<object[]> Inputs
    {
        get
        {
            return
            [
                ["02-01",     false, _now.ChangeMonth(2).ChangeDay(1).StartOfDay()],
                ["02-01",     true,  _now.ChangeMonth(2).ChangeDay(1).EndOfDay()],
                ["11-06",     false, _now.ChangeMonth(11).ChangeDay(6).StartOfDay()],
                ["11-06",     true,  _now.ChangeMonth(11).ChangeDay(6).EndOfDay()],
                ["12-24",     false, _now.ChangeMonth(12).ChangeDay(24).StartOfDay()],
                ["12-24",     true,  _now.ChangeMonth(12).ChangeDay(24).EndOfDay()],
                ["12-45",     true,  null],
                ["blah",      false, null],
                ["blah blah", true,  null]
            ];
        }
    }
}
