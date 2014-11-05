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
    }
}
