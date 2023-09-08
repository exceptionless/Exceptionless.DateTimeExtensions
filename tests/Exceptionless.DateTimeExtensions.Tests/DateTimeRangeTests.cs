using System;
using System.Collections.Generic;
using Xunit;

namespace Exceptionless.DateTimeExtensions.Tests {
    public class DateTimeRangeTests {
        private static readonly DateTime _now = new(2014, 11, 6, 4, 32, 56, 78);

        [Fact]
        public void CanCompareForEquality() {
            var range1 = new DateTimeRange(_now, _now.AddMinutes(1));
            var range2 = new DateTimeRange(_now, _now.AddMinutes(1));
            Assert.True(range1 == range2);
        }

        [Fact]
        public void CanParseNull() {
            var range = DateTimeRange.Parse(null, _now);
            Assert.Equal(DateTimeRange.Empty, range);
        }

        [Fact]
        public void CanAddAndSubtract() {
            var range1 = new DateTimeRange(_now, _now.AddMinutes(1));
            var utcRange =  range1.Add(TimeSpan.FromHours(6));
            Assert.Equal(_now.AddHours(6), utcRange.Start);
            Assert.Equal(_now.AddHours(6).AddMinutes(1), utcRange.End);

            var localRange = utcRange.Subtract(TimeSpan.FromHours(6));
            Assert.Equal(_now, localRange.Start);
            Assert.Equal(_now.AddMinutes(1), localRange.End);
        }

        [Fact]
        public void CanParseIntoLocalTime() {
            const string time = "2016-12-28T05:00:00-2016-12-28T05:30:00";
            var utcOffset = TimeSpan.FromHours(-1);
            var localRange = DateTimeRange.Parse(time, DateTimeOffset.UtcNow.ChangeOffset(utcOffset));
            Assert.Equal(new DateTime(2016, 12, 28, 5, 0, 0, DateTimeKind.Unspecified), localRange.Start);
            Assert.Equal(new DateTime(2016, 12, 28, 5, 30, 0, DateTimeKind.Unspecified), localRange.End);
            Assert.Equal(new DateTime(2016, 12, 28, 6, 0, 0, DateTimeKind.Utc), localRange.UtcStart);
            Assert.Equal(new DateTime(2016, 12, 28, 6, 30, 0, DateTimeKind.Utc), localRange.UtcEnd);
        }

        [Theory]
        [MemberData(nameof(Inputs))]
        public void CanParseNamedRanges(string input, DateTime start, DateTime end) {
            var range = DateTimeRange.Parse(input, _now);
            Assert.Equal(start, range.Start);
            Assert.Equal(end, range.End);
        }

        public static IEnumerable<object[]> Inputs => new[] {
            new object[] { "today",          _now.StartOfDay(), _now.EndOfDay() },
            new object[] { "yesterday",      _now.SubtractDays(1).StartOfDay(), _now.SubtractDays(1).EndOfDay() },
            new object[] { "tomorrow",       _now.AddDays(1).StartOfDay(), _now.AddDays(1).EndOfDay() },
            new object[] { "last 5 minutes", _now.SubtractMinutes(5).StartOfMinute(), _now },
            new object[] { "this 5 minutes", _now, _now.AddMinutes(5).EndOfMinute() },
            new object[] { "next 5 minutes", _now, _now.AddMinutes(5).EndOfMinute() },
            new object[] { "last jan",       _now.ChangeMonth(1).StartOfMonth(), _now.ChangeMonth(1).EndOfMonth() },
            new object[] { "jan",            _now.ChangeMonth(1).StartOfMonth(), _now.ChangeMonth(1).EndOfMonth() },
            new object[] { "noVemBer",       _now.StartOfMonth(), _now.EndOfMonth() },
            new object[] { "deC",            _now.ChangeMonth(12).StartOfMonth(), _now.ChangeMonth(12).EndOfMonth() },
            new object[] { "next deC",       _now.ChangeMonth(12).StartOfMonth(), _now.ChangeMonth(12).EndOfMonth() },
            new object[] { "next nov",       _now.AddYears(1).StartOfMonth(), _now.AddYears(1).EndOfMonth() },
            new object[] { "next jan",       _now.AddYears(1).ChangeMonth(1).StartOfMonth(), _now.AddYears(1).ChangeMonth(1).EndOfMonth() },
            new object[] { "jan-feb",        _now.ChangeMonth(1).StartOfMonth(), _now.ChangeMonth(2).EndOfMonth() },
            new object[] { "now-this feb",   _now, _now.AddYears(1).ChangeMonth(2).EndOfMonth() }
        };
    }
}
