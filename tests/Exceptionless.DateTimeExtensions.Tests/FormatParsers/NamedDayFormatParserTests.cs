using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers;
using Xunit;
using Xunit.Abstractions;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers;

public class NamedDayFormatParserTests : FormatParserTestsBase
{
    public NamedDayFormatParserTests(ITestOutputHelper output) : base(output) { }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void ParseInput(string input, DateTime? start, DateTime? end)
    {
        ValidateInput(new NamedDayFormatParser(), input, start, end);
    }

    public static IEnumerable<object[]> Inputs
    {
        get
        {
            return
            [
                ["yesterday", _now.SubtractDays(1).StartOfDay(), _now.SubtractDays(1).EndOfDay()],
                ["today",     _now.StartOfDay(), _now.EndOfDay()],
                ["tomorrow",  _now.AddDays(1).StartOfDay(), _now.AddDays(1).EndOfDay()],
                ["blah",      null, null],
                ["blah blah", null, null]
            ];
        }
    }
}
