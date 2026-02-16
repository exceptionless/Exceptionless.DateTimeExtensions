using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers;
using Xunit;

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
            return
            [
                // Original dash delimiter syntax
                ["2012-2013",        _now.ChangeYear(2012).StartOfYear(), _now.ChangeYear(2013).EndOfYear()],
                ["5 days ago - now", _now.SubtractDays(5).StartOfDay(), _now],
                ["jan-feb",          _now.ChangeMonth(1).StartOfMonth(), _now.ChangeMonth(2).EndOfMonth()],
                ["now-this feb",     _now, _now.AddYears(1).ChangeMonth(2).EndOfMonth()],

                // TO delimiter syntax (case-insensitive)
                ["2012 TO 2013",     _now.ChangeYear(2012).StartOfYear(), _now.ChangeYear(2013).EndOfYear()],
                ["jan to feb",       _now.ChangeMonth(1).StartOfMonth(), _now.ChangeMonth(2).EndOfMonth()],
                ["5 days ago TO now", _now.SubtractDays(5).StartOfDay(), _now],

                // Elasticsearch inclusive bracket syntax [inclusive TO inclusive]
                ["[2012 TO 2013]",   _now.ChangeYear(2012).StartOfYear(), _now.ChangeYear(2013).EndOfYear()],
                ["[jan TO feb]",     _now.ChangeMonth(1).StartOfMonth(), _now.ChangeMonth(2).EndOfMonth()],
                ["[2012-2013]",      _now.ChangeYear(2012).StartOfYear(), _now.ChangeYear(2013).EndOfYear()],

                // Elasticsearch exclusive bracket syntax {exclusive TO exclusive}
                // Exclusive min rounds up (end of period), exclusive max rounds down (start of period)
                ["{jan TO feb}",     _now.ChangeMonth(1).EndOfMonth(), _now.ChangeMonth(2).StartOfMonth()],
                ["{2012 TO 2013}",   _now.ChangeYear(2012).EndOfYear(), _now.ChangeYear(2013).StartOfYear()],

                // Mixed bracket syntax [inclusive TO exclusive}
                ["[2012 TO 2013}",   _now.ChangeYear(2012).StartOfYear(), _now.ChangeYear(2013).StartOfYear()],
                ["[jan TO feb}",     _now.ChangeMonth(1).StartOfMonth(), _now.ChangeMonth(2).StartOfMonth()],

                // Mixed bracket syntax {exclusive TO inclusive]
                ["{2012 TO 2013]",   _now.ChangeYear(2012).EndOfYear(), _now.ChangeYear(2013).EndOfYear()],
                ["{jan TO feb]",     _now.ChangeMonth(1).EndOfMonth(), _now.ChangeMonth(2).EndOfMonth()],

                // Wildcard support — wildcards always return min/max regardless of bracket type
                ["* TO 2013",        DateTime.MinValue, _now.ChangeYear(2013).EndOfYear()],
                ["2012 TO *",        _now.ChangeYear(2012).StartOfYear(), DateTime.MaxValue],
                ["[* TO 2013]",      DateTime.MinValue, _now.ChangeYear(2013).EndOfYear()],
                ["{* TO 2013}",      DateTime.MinValue, _now.ChangeYear(2013).StartOfYear()],
                ["{2012 TO *}",      _now.ChangeYear(2012).EndOfYear(), DateTime.MaxValue],
                ["[2012 TO *}",      _now.ChangeYear(2012).StartOfYear(), DateTime.MaxValue],

                // Bracket-aware rounding with /d — all four bracket combinations
                // [ = inclusive (gte) rounds to start of day, ] = inclusive (lte) rounds to end of day
                // { = exclusive (gt) rounds to end of day, } = exclusive (lt) rounds to start of day
                ["[now-7d/d TO now/d]",  _now.SubtractDays(7).StartOfDay(), _now.EndOfDay()],
                ["{now-7d/d TO now/d}",  _now.SubtractDays(7).EndOfDay(), _now.StartOfDay()],
                ["[now-7d/d TO now/d}",  _now.SubtractDays(7).StartOfDay(), _now.StartOfDay()],
                ["{now-7d/d TO now/d]",  _now.SubtractDays(7).EndOfDay(), _now.EndOfDay()],

                // Bracket-aware rounding with /M — all four bracket combinations
                ["[now/M TO now+2M/M]",  _now.StartOfMonth(), _now.AddMonths(2).EndOfMonth()],
                ["{now/M TO now+2M/M}",  _now.EndOfMonth(), _now.AddMonths(2).StartOfMonth()],
                ["[now/M TO now+2M/M}",  _now.StartOfMonth(), _now.AddMonths(2).StartOfMonth()],
                ["{now/M TO now+2M/M]",  _now.EndOfMonth(), _now.AddMonths(2).EndOfMonth()],

                // Bracket-aware rounding with /h — all four bracket combinations
                ["[now-3h/h TO now/h]",  _now.AddHours(-3).StartOfHour(), _now.EndOfHour()],
                ["{now-3h/h TO now/h}",  _now.AddHours(-3).EndOfHour(), _now.StartOfHour()],
                ["[now-3h/h TO now/h}",  _now.AddHours(-3).StartOfHour(), _now.StartOfHour()],
                ["{now-3h/h TO now/h]",  _now.AddHours(-3).EndOfHour(), _now.EndOfHour()],

                // Bracket-aware rounding with /y — all four bracket combinations
                ["[now-2y/y TO now/y]",  _now.AddYears(-2).StartOfYear(), _now.EndOfYear()],
                ["{now-2y/y TO now/y}",  _now.AddYears(-2).EndOfYear(), _now.StartOfYear()],
                ["[now-2y/y TO now/y}",  _now.AddYears(-2).StartOfYear(), _now.StartOfYear()],
                ["{now-2y/y TO now/y]",  _now.AddYears(-2).EndOfYear(), _now.EndOfYear()],

                // Invalid inputs
                ["blah",             null, null],
                ["[invalid",         null, null],
                ["invalid}",         null, null],

                // Invalid bracket validation
                ["}2012 TO 2013{",   null, null], // Wrong orientation
                ["]2012 TO 2013[",   null, null], // Wrong orientation
                ["[2012 TO 2013",    null, null], // Missing closing bracket
                ["2012 TO 2013]",    null, null], // Missing opening bracket

                // Invalid date-math units (uppercase D is NOT valid per Elastic spec)
                ["[now-7D TO now]",  null, null], // uppercase D is invalid
                ["[now-1D/D TO now]", null, null], // uppercase D with rounding
                ["[now-7d TO now]",  _now.SubtractDays(7), _now], // lowercase d is valid
            ];
        }
    }
}
