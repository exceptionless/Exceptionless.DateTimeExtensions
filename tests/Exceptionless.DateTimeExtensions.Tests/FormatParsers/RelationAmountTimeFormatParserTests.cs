using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers;
using Xunit;
using Xunit.Abstractions;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers;

public class RelationAmountTimeFormatParserTests : FormatParserTestsBase
{
    public RelationAmountTimeFormatParserTests(ITestOutputHelper output) : base(output) { }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void ParseInput(string input, DateTime? start, DateTime? end)
    {
        ValidateInput(new RelationAmountTimeFormatParser(), input, start, end);
    }

    public static IEnumerable<object[]> Inputs
    {
        get
        {
            return
            [
                ["last 5 minutes",   _now.StartOfMinute().SubtractMinutes(5), _now],
                ["last 30 days",     _now.StartOfDay().SubtractDays(30),  _now],
                ["next 5 weeks",     _now, _now.AddWeeks(5).EndOfDay()],
                ["this 3 hours",     _now, _now.AddHours(3).EndOfHour()],
                ["previous 3 years", _now.SubtractYears(3).StartOfDay(), _now],
                ["blah",             null, null],
                ["blah blah",        null,  null]
            ];
        }
    }
}
