using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers;
using Xunit;
using Xunit.Abstractions;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers;

public class TwoPartFormatParserTests : FormatParserTestsBase
{
    public TwoPartFormatParserTests(ITestOutputHelper output) : base(output) { }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void ParseInput(string input, DateTime? start, DateTime? end)
    {
        ValidateInput(new TwoPartFormatParser(), input, start, end);
    }

    public static IEnumerable<object[]> Inputs
    {
        get
        {
            return new[] {
                new object[] { "2012-2013",        _now.ChangeYear(2012).StartOfYear(), _now.ChangeYear(2013).EndOfYear() },
                new object[] { "5 days ago - now", _now.SubtractDays(5).StartOfDay(), _now },
                new object[] { "jan-feb",          _now.ChangeMonth(1).StartOfMonth(), _now.ChangeMonth(2).EndOfMonth() },
                new object[] { "now-this feb",     _now, _now.AddYears(1).ChangeMonth(2).EndOfMonth() },
                new object[] { "blah",             null, null },
                new object[] { "blah blah",        null, null }
            };
        }
    }
}
