using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers;
using Xunit;
using Xunit.Abstractions;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers;

public class YearFormatParserTests : FormatParserTestsBase
{
    public YearFormatParserTests(ITestOutputHelper output) : base(output) { }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void ParseInput(string input, DateTime? start, DateTime? end)
    {
        ValidateInput(new YearFormatParser(), input, start, end);
    }

    public static IEnumerable<object[]> Inputs
    {
        get
        {
            return new[] {
                new object[] { "2013",      _now.ChangeYear(2013).StartOfYear(), _now.ChangeYear(2013).EndOfYear() },
                ["2012",      _now.ChangeYear(2012).StartOfYear(), _now.ChangeYear(2012).EndOfYear()],
                ["blah",      null, null],
                ["blah blah", null, null]
            };
        }
    }
}
