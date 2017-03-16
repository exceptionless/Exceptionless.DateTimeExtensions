using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Exceptionless.DateTimeExtensions.Tests {
    public class TimeUnitTests {
        public static IEnumerable<object[]> TestData => new[] {
            new object[] { "1000 nanos", new TimeSpan(10) },
            new object[] { "1000nanos", new TimeSpan(10) },
            new object[] { "1000 NANOS", new TimeSpan(10) },
            new object[] { "1000NANOS", new TimeSpan(10) },
            new object[] { "10micros", new TimeSpan(100) },
            new object[] { "10ms", new TimeSpan(0, 0, 0, 0, 10) },
            new object[] { "10s", new TimeSpan(0, 0, 10) },
            new object[] { "-10s", new TimeSpan(0, 0, -10) },
            new object[] { "10m", new TimeSpan(0, 10, 0) },
            new object[] { "10h", new TimeSpan(10, 0, 0) },
            new object[] { "10d", new TimeSpan(10, 0, 0, 0) },
        };

        [Theory]
        [MemberData(nameof(TestData))]
        public void CanParse(string value, TimeSpan expected) {
            Assert.Equal(expected, TimeUnit.Parse(value));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("1.234h")] // fractional time
        [InlineData("1234")] // missing unit
        [InlineData("12unknownunit")]
        [InlineData("12h.")]
        public void VerifyParseFailure(string value) {
            Assert.ThrowsAny<Exception>(() => TimeUnit.Parse(value));
        }
    }
}