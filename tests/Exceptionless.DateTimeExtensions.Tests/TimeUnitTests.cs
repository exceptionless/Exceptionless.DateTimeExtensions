using System;
using System.Collections.Generic;
using Xunit;

namespace Exceptionless.DateTimeExtensions.Tests;

public class TimeUnitTests
{
    public static IEnumerable<object[]> TestData => new[] {
        new object[] { "1000 nanos", new TimeSpan(10) },
        ["1000nanos", new TimeSpan(10)],
        ["1000 NANOS", new TimeSpan(10)],
        ["1000NANOS", new TimeSpan(10)],
        ["10micros", new TimeSpan(100)],
        ["10ms", new TimeSpan(0, 0, 0, 0, 10)],
        ["10s", new TimeSpan(0, 0, 10)],
        ["-10s", new TimeSpan(0, 0, -10)],
        ["10m", new TimeSpan(0, 10, 0)],
        ["10h", new TimeSpan(10, 0, 0)],
        ["10d", new TimeSpan(10, 0, 0, 0)],
    };

    [Theory]
    [MemberData(nameof(TestData))]
    public void CanParse(string value, TimeSpan expected)
    {
        Assert.Equal(expected, TimeUnit.Parse(value));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("1.234h")] // fractional time
    [InlineData("1234")] // missing unit
    [InlineData("12unknownunit")]
    [InlineData("12h.")]
    public void VerifyParseFailure(string value)
    {
        Assert.ThrowsAny<Exception>(() => TimeUnit.Parse(value));
    }

    [Theory]
    [InlineData("1000 nanos", true)]
    [InlineData("1000nanos", true)]
    [InlineData("1000 NANOS", true)]
    [InlineData("1000NANOS", true)]
    [InlineData("10micros", true)]
    [InlineData("10ms", true)]
    [InlineData("10s", true)]
    [InlineData("-10s", true)]
    [InlineData("10m", true)]
    [InlineData("10h", true)]
    [InlineData("10d", true)]
    [InlineData(null, false)]
    [InlineData("1.234h", false)] // fractional time
    [InlineData("1234", false)] // missing unit
    [InlineData("12unknownunit", false)]
    [InlineData("12h.", false)]
    [InlineData("Blah/Blahs", false)]
    public void VerifyTryParse(string value, bool expected)
    {
        bool success = TimeUnit.TryParse(value, out var result);
        Assert.Equal(expected, success);
    }
}
