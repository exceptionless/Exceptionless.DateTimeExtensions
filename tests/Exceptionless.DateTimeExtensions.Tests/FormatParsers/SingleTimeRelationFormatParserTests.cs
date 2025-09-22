using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers;
using Xunit;
using Xunit.Abstractions;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers;

public class SingleTimeRelationFormatParserTests : FormatParserTestsBase
{
    public SingleTimeRelationFormatParserTests(ITestOutputHelper output) : base(output) { }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void ParseInput(string input, DateTime? start, DateTime? end)
    {
        ValidateInput(new SingleTimeRelationFormatParser(), input, start, end);
    }

    public static IEnumerable<object[]> Inputs
    {
        get
        {
            return new[] {
                new object[] { "past minute",   _now.SubtractMinutes(1).StartOfMinute(), _now },
                ["next minute",   _now, _now.AddMinutes(1).EndOfMinute()],
                ["last hour",     _now.SubtractHours(1).StartOfHour(), _now],
                ["next hour",     _now, _now.AddHours(1).EndOfHour()],
                ["previous day",  _now.SubtractDays(1).StartOfDay(), _now],
                ["next day",      _now, _now.AddDays(1).EndOfDay()],
                ["next month",    _now, _now.AddMonths(1).EndOfDay()],
                ["last month",    _now.SubtractMonths(1).StartOfDay(), _now],
                ["previous week", _now.SubtractWeeks(1).StartOfDay(), _now],
                ["next week",     _now, _now.AddWeeks(1).EndOfDay()],
                ["past year",     _now.SubtractYears(1).StartOfDay(), _now],
                ["next year",     _now, _now.AddYears(1).EndOfDay()],
                ["blah",          null, null],
                ["blah blah",     null, null]
            };
        }
    }
}
