using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;
using Xunit;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers.PartParsers;

public class MonthPartParserTests : PartParserTestsBase
{
    public MonthPartParserTests(ITestOutputHelper output) : base(output) { }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void ParseInput(string input, bool isUpperLimit, DateTimeOffset? expected)
    {
        ValidateInput(new MonthPartParser(), input, isUpperLimit, expected);
    }

    public static IEnumerable<object[]> Inputs
    {
        get
        {
            return
            [
                ["jan",       false, _now.ChangeMonth(1).StartOfMonth()],
                ["jan",       true,  _now.ChangeMonth(1).EndOfMonth()],
                ["nov",       false, _now.StartOfMonth()],
                ["nov",       true,  _now.EndOfMonth()],
                ["decemBer",  false, _now.ChangeMonth(12).StartOfMonth()],
                ["dec",       true,  _now.ChangeMonth(12).EndOfMonth()],
                ["blah",      false, null],
                ["blah blah", true,  null]
            ];
        }
    }
}
