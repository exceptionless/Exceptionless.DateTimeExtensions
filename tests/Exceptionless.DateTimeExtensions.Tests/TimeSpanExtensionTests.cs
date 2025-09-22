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
    public void SubtractSaturating()
    {
        // Basic case: normal subtraction when result is positive
        var timeSpan1 = TimeSpan.FromHours(5);
        var timeSpan2 = TimeSpan.FromHours(2);
        Assert.Equal(TimeSpan.FromHours(3), timeSpan1.SubtractSaturating(timeSpan2));

        // Saturating case: result would be negative, should return TimeSpan.Zero
        var timeSpan3 = TimeSpan.FromHours(2);
        var timeSpan4 = TimeSpan.FromHours(5);
        Assert.Equal(TimeSpan.Zero, timeSpan3.SubtractSaturating(timeSpan4));

        // Edge case: equal values should return TimeSpan.Zero
        var timeSpan5 = TimeSpan.FromMinutes(30);
        var timeSpan6 = TimeSpan.FromMinutes(30);
        Assert.Equal(TimeSpan.Zero, timeSpan5.SubtractSaturating(timeSpan6));

        // Zero cases
        Assert.Equal(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10).SubtractSaturating(TimeSpan.Zero));
        Assert.Equal(TimeSpan.Zero, TimeSpan.Zero.SubtractSaturating(TimeSpan.FromSeconds(5)));
        Assert.Equal(TimeSpan.Zero, TimeSpan.Zero.SubtractSaturating(TimeSpan.Zero));

        // Large values
        var largeTimeSpan1 = TimeSpan.FromDays(100);
        var largeTimeSpan2 = TimeSpan.FromDays(50);
        Assert.Equal(TimeSpan.FromDays(50), largeTimeSpan1.SubtractSaturating(largeTimeSpan2));

        // Negative TimeSpan values - testing with negative self value
        var negativeTimeSpan = TimeSpan.FromHours(-2);
        var positiveTimeSpan = TimeSpan.FromHours(1);
        Assert.Equal(TimeSpan.Zero, negativeTimeSpan.SubtractSaturating(positiveTimeSpan));

        // Negative other value (subtracting negative is like adding)
        var timeSpan7 = TimeSpan.FromHours(3);
        var negativeTimeSpan2 = TimeSpan.FromHours(-2);
        Assert.Equal(TimeSpan.FromHours(5), timeSpan7.SubtractSaturating(negativeTimeSpan2));
    }
}
