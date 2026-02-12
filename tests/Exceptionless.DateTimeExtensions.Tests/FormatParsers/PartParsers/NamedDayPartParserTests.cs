using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;
using Xunit;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers.PartParsers;

public class NamedDayPartParserTests : PartParserTestsBase
{
    public NamedDayPartParserTests(ITestOutputHelper output) : base(output) { }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void ParseInput(string input, bool isUpperLimit, DateTimeOffset? expected)
    {
        ValidateInput(new NamedDayPartParser(), input, isUpperLimit, expected);
    }

    public static IEnumerable<object[]> Inputs
    {
        get
        {
            return
            [
                ["now",       false, _now],
                ["now",       true,  _now],
                ["yesterday", false, _now.SubtractDays(1).StartOfDay()],
                ["yesterday", true,  _now.SubtractDays(1).EndOfDay()],
                ["today",     false, _now.StartOfDay()],
                ["today",     true,  _now.EndOfDay()],
                ["tomorrow",  false, _now.AddDays(1).StartOfDay()],
                ["tomorrow",  true,  _now.AddDays(1).EndOfDay()],
                ["blah",      false, null],
                ["blah blah", true,  null]
            ];
        }
    }
}
