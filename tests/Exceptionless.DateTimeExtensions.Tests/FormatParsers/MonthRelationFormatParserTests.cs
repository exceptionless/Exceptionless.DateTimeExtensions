using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers;
using Xunit;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers;

public class MonthRelationFormatParserTests : FormatParserTestsBase
{
    public MonthRelationFormatParserTests(ITestOutputHelper output) : base(output) { }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void ParseInput(string input, DateTime? start, DateTime? end)
    {
        ValidateInput(new MonthRelationFormatParser(), input, start, end);
    }

    public static IEnumerable<object[]> Inputs
    {
        get
        {
            return
            [
                ["this janUary",  _now.AddYears(1).ChangeMonth(1).StartOfMonth(), _now.AddYears(1).ChangeMonth(1).EndOfMonth()
                ],
                ["last jan",      _now.ChangeMonth(1).StartOfMonth(), _now.ChangeMonth(1).EndOfMonth()],
                ["next januaRY",  _now.AddYears(1).ChangeMonth(1).StartOfMonth(), _now.AddYears(1).ChangeMonth(1).EndOfMonth()],
                ["this november", _now.StartOfMonth(), _now.EndOfMonth()],
                ["next november", _now.AddYears(1).StartOfMonth(), _now.AddYears(1).EndOfMonth()],
                ["this december", _now.ChangeMonth(12).StartOfMonth(), _now.ChangeMonth(12).EndOfMonth()],
                ["last november", _now.SubtractYears(1).StartOfMonth(), _now.SubtractYears(1).EndOfMonth()],
                ["blah",          null, null],
                ["blah blah",     null, null]
            ];
        }
    }
}
