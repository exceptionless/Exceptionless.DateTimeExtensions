using System;
using Xunit;

namespace Exceptionless.DateTimeExtensions.Tests {
    public class DateTimeExtensionsTests {
        [Fact]
        public void ToAge() {
            var date = DateTime.Now.SubtractMinutes(5);
            Assert.Equal("5 minutes", date.ToAgeString());

            date = DateTime.Now.SubtractDays(3).SubtractMinutes(12);
            Assert.Equal("3 days 12 minutes", date.ToAgeString());
        }

        [Fact]
        public void EndOfHour() {
            Assert.Equal(new DateTime(2014, 1, 1, 5, 59, 59, 999), new DateTime(2014, 1, 1, 5, 0, 0, 1).EndOfHour());
            Assert.Equal(new DateTime(2014, 1, 1, 5, 59, 59, 999), new DateTime(2014, 1, 1, 5, 0, 0, 0).EndOfHour());
        }

        [Fact]
        public void EndOfDay() {
            Assert.Equal(new DateTime(2014, 11, 7).Subtract(TimeSpan.FromMilliseconds(1)), new DateTime(2014, 11, 6).EndOfDay());
            Assert.Equal(new DateTime(2014, 11, 8).Subtract(TimeSpan.FromMilliseconds(1)), new DateTime(2014, 11, 6, 1, 12, 12).AddDays(1).EndOfDay());

            Assert.Equal(new DateTime(2014, 11, 6), new DateTime(2014, 11, 6).StartOfDay());
            Assert.Equal(new DateTime(2011, 11, 6), new DateTime(2014, 11, 6).SubtractYears(3).StartOfDay());
        }

        [Fact]
        public void SafeAdd() {
            Assert.Equal(DateTime.MinValue.AddHours(1), DateTime.MinValue.SafeAdd(TimeSpan.FromHours(1)));
            Assert.Equal(DateTime.MinValue, DateTime.MinValue.SafeAdd(TimeSpan.FromHours(-1)));
            Assert.Equal(DateTime.MaxValue, DateTime.MaxValue.SafeAdd(TimeSpan.FromHours(1)));
            Assert.Equal(DateTime.MaxValue, DateTime.MaxValue.SubtractHours(1).SafeAdd(TimeSpan.FromHours(1)));
            Assert.Equal(DateTime.MaxValue, DateTime.MaxValue.SubtractHours(1).SafeAdd(TimeSpan.FromHours(2)));
        }

        [Fact]
        public void SafeSubtract() {
            Assert.Equal(DateTime.MinValue, DateTime.MinValue.SafeSubtract(TimeSpan.FromHours(1)));
            Assert.Equal(DateTime.MaxValue.SubtractHours(1), DateTime.MaxValue.SafeSubtract(TimeSpan.FromHours(1)));
        }
    }
}
