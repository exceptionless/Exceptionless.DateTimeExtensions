using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;
using Xunit;
using Xunit.Abstractions;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers.PartParsers;

public class MonthRelationPartParserTests : PartParserTestsBase
{
    public MonthRelationPartParserTests(ITestOutputHelper output) : base(output) { }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void ParseInput(string input, bool isUpperLimit, DateTimeOffset? expected)
    {
        ValidateInput(new MonthRelationPartParser(), input, isUpperLimit, expected);
    }

    public static IEnumerable<object[]> Inputs
    {
        get
        {
            return
            [
                ["this jan",      false, _now.AddYears(1).ChangeMonth(1).StartOfMonth()],
                ["this janUary",  true,  _now.AddYears(1).ChangeMonth(1).EndOfMonth()],
                ["last jan",      false, _now.ChangeMonth(1).StartOfMonth()],
                ["last jan",      true,  _now.ChangeMonth(1).EndOfMonth()],
                ["next januaRY",  false, _now.AddYears(1).ChangeMonth(1).StartOfMonth()],
                ["next jan",      true,  _now.AddYears(1).ChangeMonth(1).EndOfMonth()],
                ["this november", false, _now.StartOfMonth()],
                ["this november", true,  _now.EndOfMonth()],
                ["next november", false, _now.AddYears(1).StartOfMonth()],
                ["next november", true,  _now.AddYears(1).EndOfMonth()],
                ["this december", true,  _now.ChangeMonth(12).EndOfMonth()],
                ["last november", false, _now.SubtractYears(1).StartOfMonth()],
                ["blah",          false, null],
                ["blah blah",     true,  null]
            ];
        }
    }
}
