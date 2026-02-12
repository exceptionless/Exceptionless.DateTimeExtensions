using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;
using Xunit;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers.PartParsers;

public class SingleTimeRelationPartParserTests : PartParserTestsBase
{
    public SingleTimeRelationPartParserTests(ITestOutputHelper output) : base(output) { }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void ParseInput(string input, bool isUpperLimit, DateTimeOffset? expected)
    {
        ValidateInput(new SingleTimeRelationPartParser(), input, isUpperLimit, expected);
    }

    public static IEnumerable<object[]> Inputs
    {
        get
        {
            return
            [
                ["a minute ago",      false, _now.SubtractMinutes(1).StartOfMinute()],
                ["a minute ago",      true,  _now.SubtractMinutes(1).EndOfMinute()],
                ["a minute from now", false, _now.AddMinutes(1).StartOfMinute()],
                ["a minute from now", true,  _now.AddMinutes(1).EndOfMinute()],
                ["an hour ago",       false, _now.SubtractHours(1).StartOfHour()],
                ["an hour ago",       true,  _now.SubtractHours(1).EndOfHour()],
                ["an hour from now",  false, _now.AddHours(1).StartOfHour()],
                ["an hour from now",  true,  _now.AddHours(1).EndOfHour()],
                ["a day ago",         false, _now.SubtractDays(1).StartOfDay()],
                ["a day ago",         true,  _now.SubtractDays(1).EndOfDay()],
                ["a day from now",    false, _now.AddDays(1).StartOfDay()],
                ["a day from now",    true,  _now.AddDays(1).EndOfDay()],
                ["a month ago",       false, _now.SubtractMonths(1).StartOfDay()],
                ["a month ago",       true,  _now.SubtractMonths(1).EndOfDay()],
                ["a month from now",  false, _now.AddMonths(1).StartOfDay()],
                ["a month from now",  true,  _now.AddMonths(1).EndOfDay()],
                ["a week ago",        false, _now.SubtractWeeks(1).StartOfDay()],
                ["a week ago",        true,  _now.SubtractWeeks(1).EndOfDay()],
                ["a week from now",   false, _now.AddWeeks(1).StartOfDay()],
                ["a week from now",   true,  _now.AddWeeks(1).EndOfDay()],
                ["a year ago",        false, _now.SubtractYears(1).StartOfDay()],
                ["a year ago",        true,  _now.SubtractYears(1).EndOfDay()],
                ["a year from now",   false, _now.AddYears(1).StartOfDay()],
                ["a year from now",   true,  _now.AddYears(1).EndOfDay()],
                ["blah",              false, null],
                ["blah blah",         true,  null]
            ];
        }
    }
}
