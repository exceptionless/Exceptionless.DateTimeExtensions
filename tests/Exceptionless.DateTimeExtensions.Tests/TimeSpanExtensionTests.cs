using System;
using Xunit;

namespace Exceptionless.DateTimeExtensions.Tests;

public class TimeSpanExtensionTests
{
    [Fact]
    public void ToWords()
    {
        var value = TimeSpan.FromMilliseconds(100);
        Assert.Equal("100 milliseconds", value.ToWords());

        value = TimeSpan.FromMilliseconds(-100);
        Assert.Equal("-100 milliseconds", value.ToWords());

        value = TimeSpan.FromMilliseconds(100);
        Assert.Equal("100ms", value.ToWords(true));

        value = TimeSpan.FromMilliseconds(2500);
        Assert.Equal("2.5 seconds", value.ToWords());

        value = TimeSpan.FromMilliseconds(2500);
        Assert.Equal("2.5s", value.ToWords(true));

        value = TimeSpan.FromMilliseconds(16500);
        Assert.Equal("16 seconds", value.ToWords());

        value = TimeSpan.FromMilliseconds(16500);
        Assert.Equal("16s", value.ToWords(true));

        value = TimeSpan.FromHours(6);
        Assert.Equal("6 hours", value.ToWords());

        value = TimeSpan.FromHours(-6);
        Assert.Equal("-6 hours", value.ToWords());

        value = TimeSpan.FromHours(6);
        Assert.Equal("6h", value.ToWords(true));

        value = TimeSpan.FromMinutes(186);
        Assert.Equal("3 hours 6 minutes", value.ToWords());

        value = TimeSpan.FromMinutes(-186);
        Assert.Equal("-3 hours 6 minutes", value.ToWords());

        value = TimeSpan.FromMinutes(186);
        Assert.Equal("3h 6m", value.ToWords(true));

        value = TimeSpan.FromDays(10.15);
        Assert.Equal("1 week 3 days", value.ToWords(false, 2));

        value = TimeSpan.FromDays(10.15);
        Assert.Equal("1w 3d 3h 36m", value.ToWords(true));
    }

    [Fact]
    public void ApproximateAge()
    {
        Assert.Equal("Just now", DateTime.Now.AddMilliseconds(-100).ToApproximateAgeString());
        Assert.Equal("Just now", DateTime.Now.AddSeconds(-50).ToApproximateAgeString());
        Assert.Equal("1 minute ago", DateTime.Now.AddSeconds(-65).ToApproximateAgeString());
        Assert.Equal("1 hour ago", DateTime.Now.AddMinutes(-61).ToApproximateAgeString());
        Assert.Equal("1 day ago", DateTime.Now.AddHours(-24).ToApproximateAgeString());
        Assert.Equal("2 days ago", DateTime.Now.AddHours(-48).ToApproximateAgeString());
        Assert.Equal("1 week ago", DateTime.Now.AddDays(-7).ToApproximateAgeString());
    }

    [Fact]
    public void SubtractSaturating_WithLargerValue_ReturnsNormalSubtraction()
    {
        var timeSpan1 = TimeSpan.FromHours(5);
        var timeSpan2 = TimeSpan.FromHours(2);

        var result = timeSpan1.SubtractSaturating(timeSpan2);

        Assert.Equal(TimeSpan.FromHours(3), result);
    }

    [Fact]
    public void SubtractSaturating_WithSmallerValue_ReturnsZero()
    {
        var timeSpan1 = TimeSpan.FromHours(2);
        var timeSpan2 = TimeSpan.FromHours(5);

        var result = timeSpan1.SubtractSaturating(timeSpan2);

        Assert.Equal(TimeSpan.Zero, result);
    }

    [Fact]
    public void SubtractSaturating_WithEqualValues_ReturnsZero()
    {
        var timeSpan1 = TimeSpan.FromMinutes(30);
        var timeSpan2 = TimeSpan.FromMinutes(30);

        var result = timeSpan1.SubtractSaturating(timeSpan2);

        Assert.Equal(TimeSpan.Zero, result);
    }

    [Fact]
    public void SubtractSaturating_WithZeroValue_ReturnsOriginalValue()
    {
        var timeSpan = TimeSpan.FromSeconds(10);

        var result = timeSpan.SubtractSaturating(TimeSpan.Zero);

        Assert.Equal(TimeSpan.FromSeconds(10), result);
    }

    [Fact]
    public void SubtractSaturating_WithZeroAsBase_ReturnsZero()
    {
        var result1 = TimeSpan.Zero.SubtractSaturating(TimeSpan.FromSeconds(5));
        var result2 = TimeSpan.Zero.SubtractSaturating(TimeSpan.Zero);

        Assert.Equal(TimeSpan.Zero, result1);
        Assert.Equal(TimeSpan.Zero, result2);
    }

    [Fact]
    public void SubtractSaturating_WithLargeValues_ReturnsCorrectResult()
    {
        var largeTimeSpan1 = TimeSpan.FromDays(100);
        var largeTimeSpan2 = TimeSpan.FromDays(50);

        var result = largeTimeSpan1.SubtractSaturating(largeTimeSpan2);

        Assert.Equal(TimeSpan.FromDays(50), result);
    }

    [Fact]
    public void SubtractSaturating_WithNegativeBase_ReturnsZero()
    {
        var negativeTimeSpan = TimeSpan.FromHours(-2);
        var positiveTimeSpan = TimeSpan.FromHours(1);

        var result = negativeTimeSpan.SubtractSaturating(positiveTimeSpan);

        Assert.Equal(TimeSpan.Zero, result);
    }

    [Fact]
    public void SubtractSaturating_WithNegativeSubtrahend_ReturnsAddedValue()
    {
        var timeSpan = TimeSpan.FromHours(3);
        var negativeTimeSpan = TimeSpan.FromHours(-2);

        var result = timeSpan.SubtractSaturating(negativeTimeSpan);

        Assert.Equal(TimeSpan.FromHours(5), result);
    }
}
