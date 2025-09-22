using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;
using Xunit;
using Xunit.Abstractions;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers.PartParsers;

public class ExplicitDatePartParserTests : PartParserTestsBase
{
    public ExplicitDatePartParserTests(ITestOutputHelper output) : base(output) { }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void ParseInput(string input, bool isUpperLimit, DateTimeOffset? expected)
    {
        ValidateInput(new ExplicitDatePartParser(), input, isUpperLimit, expected);
    }

    public static IEnumerable<object[]> Inputs
    {
        get
        {
            return
            [
                ["2014-02-01",               false, _now.Change(null, 2, 1).StartOfDay()],
                ["2014-02-01",               true,  _now.Change(null, 2, 1).EndOfDay()],
                ["2014-02-01T05",            false, _now.Change(null, 2, 1, 5).StartOfHour()],
                ["2014-02-01T05",            true,  _now.Change(null, 2, 1, 5).EndOfHour()],
                ["2014-02-01T05:30",         false, _now.Change(null, 2, 1, 5, 30).StartOfMinute()],
                ["2014-02-01T05:30",         true,  _now.Change(null, 2, 1, 5, 30).EndOfMinute()],
                ["2014-02-01T05:30:20",      false, _now.Change(null, 2, 1, 5, 30, 20).StartOfSecond()],
                ["2014-02-01T05:30:20",      true,  _now.Change(null, 2, 1, 5, 30, 20).StartOfSecond()],
                ["2014-02-01T05:30:20.000",  false, _now.Change(null, 2, 1, 5, 30, 20).StartOfSecond()],
                ["2014-02-01T05:30:20.999",  true,  _now.Change(null, 2, 1, 5, 30, 20).EndOfSecond()],
                ["2014-02-01T05:30:20.000Z", false, _now.Change(null, 2, 1, 5, 30, 20).StartOfSecond()],
                ["2014-02-01T05:30:20.999Z", true,  _now.Change(null, 2, 1, 5, 30, 20).EndOfSecond()],
                ["2014-11-06",               false, _now.Change(null, 11, 6).StartOfDay()],
                ["2014-11-06",               true,  _now.Change(null, 11, 6).EndOfDay()],
                ["2014-12-24",               false, _now.Change(null, 12, 24).StartOfDay()],
                ["2014-12-24",               true,  _now.Change(null, 12, 24).EndOfDay()],
                ["2014-12-45",               true,  null],
                ["blah",                     false, null],
                ["blah blah",                true,  null]
            ];
        }
    }
}
