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
                // Original dash delimiter syntax
                new object[] { "2012-2013",        _now.ChangeYear(2012).StartOfYear(), _now.ChangeYear(2013).EndOfYear() },
                ["5 days ago - now", _now.SubtractDays(5).StartOfDay(), _now],
                ["jan-feb",          _now.ChangeMonth(1).StartOfMonth(), _now.ChangeMonth(2).EndOfMonth()],
                ["now-this feb",     _now, _now.AddYears(1).ChangeMonth(2).EndOfMonth()],

                // TO delimiter syntax (case-insensitive)
                ["2012 TO 2013",     _now.ChangeYear(2012).StartOfYear(), _now.ChangeYear(2013).EndOfYear()],
                ["jan to feb",       _now.ChangeMonth(1).StartOfMonth(), _now.ChangeMonth(2).EndOfMonth()],
                ["5 days ago TO now", _now.SubtractDays(5).StartOfDay(), _now],

                // Elasticsearch bracket syntax
                ["[2012 TO 2013]",   _now.ChangeYear(2012).StartOfYear(), _now.ChangeYear(2013).EndOfYear()],
                ["{jan TO feb}",     _now.ChangeMonth(1).StartOfMonth(), _now.ChangeMonth(2).EndOfMonth()],
                ["[2012-2013]",      _now.ChangeYear(2012).StartOfYear(), _now.ChangeYear(2013).EndOfYear()],

                // Wildcard support
                ["* TO 2013",        DateTime.MinValue, _now.ChangeYear(2013).EndOfYear()],
                ["2012 TO *",        _now.ChangeYear(2012).StartOfYear(), DateTime.MaxValue],
                ["[* TO 2013]",      DateTime.MinValue, _now.ChangeYear(2013).EndOfYear()],
                ["{2012 TO *}",      _now.ChangeYear(2012).StartOfYear(), DateTime.MaxValue],

                // Invalid inputs
                ["blah",             null, null],
                ["[invalid",         null, null],
                ["invalid}",         null, null]
            };
        }
    }
}
