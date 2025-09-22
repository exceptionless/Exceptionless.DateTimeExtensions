using System;
using System.Diagnostics;
using Xunit;

namespace Exceptionless.DateTimeExtensions.Tests;

public class BusinessDayTests
{
    [Fact]
    public void BusinessHours()
    {
        var date = DateTime.Now.StartOfDay().AddHours(8);

        var day = new BusinessDay(date.Date.DayOfWeek, date.Subtract(TimeSpan.FromHours(1)).TimeOfDay, date.AddHours(1).TimeOfDay);

        bool isDay = day.IsBusinessDay(date);
        Assert.True(isDay);

        // day of week test
        isDay = day.IsBusinessDay(date.AddDays(1));
        Assert.False(isDay);

        // to early test
        isDay = day.IsBusinessDay(date.Date);
        Assert.False(isDay);

        // to late test
        isDay = day.IsBusinessDay(date.AddHours(2));
        Assert.False(isDay);
    }

    [Fact]
    public void TotalBusinessHours()
    {
        var startDate = new DateTime(2010, 1, 1);
        var endDate = new DateTime(2010, 1, 2);
        var businessWeek = BusinessWeek.DefaultWeek;

        var time = businessWeek.GetBusinessTime(startDate, endDate);
        // workday friday
        Assert.Equal(8, time.TotalHours);

        startDate = new DateTime(2010, 1, 1);
        endDate = new DateTime(2010, 1, 3);

        time = businessWeek.GetBusinessTime(startDate, endDate);
        // workday friday
        Assert.Equal(8, time.TotalHours);

        startDate = new DateTime(2010, 1, 1, 12, 0, 0);
        endDate = new DateTime(2010, 1, 1, 16, 0, 0);

        time = businessWeek.GetBusinessTime(startDate, endDate);
        Assert.Equal(4, time.TotalHours);

        startDate = new DateTime(2010, 1, 1, 6, 0, 0);
        endDate = new DateTime(2010, 1, 1, 12, 0, 0);

        time = businessWeek.GetBusinessTime(startDate, endDate);
        Assert.Equal(3, time.TotalHours);

        startDate = new DateTime(2010, 1, 3, 0, 0, 0);
        endDate = new DateTime(2010, 1, 10, 0, 0, 0);

        time = businessWeek.GetBusinessTime(startDate, endDate);
        Assert.Equal(40, time.TotalHours);
    }

    [Fact]
    public void NextBusinessDay()
    {
        var businessWeek = BusinessWeek.DefaultWeek;

        bool result = businessWeek.NextBusinessDay(new DateTime(2010, 1, 1, 2, 0, 0), out var resultDate, out var businessDay);
        Assert.True(result);
        Assert.Equal(new DateTime(2010, 1, 1, 9, 0, 0), resultDate);
        Assert.NotNull(businessDay);
        Assert.Equal(DayOfWeek.Friday, businessDay.DayOfWeek);

        result = businessWeek.NextBusinessDay(new DateTime(2010, 1, 1, 11, 0, 0), out resultDate, out businessDay);
        Assert.True(result);
        Assert.Equal(new DateTime(2010, 1, 1, 11, 0, 0), resultDate);
        Assert.NotNull(businessDay);
        Assert.Equal(DayOfWeek.Friday, businessDay.DayOfWeek);

        result = businessWeek.NextBusinessDay(new DateTime(2010, 1, 2, 11, 0, 0), out resultDate, out businessDay);
        Assert.True(result);
        Assert.Equal(new DateTime(2010, 1, 4, 9, 0, 0), resultDate);
        Assert.NotNull(businessDay);
        Assert.Equal(DayOfWeek.Monday, businessDay.DayOfWeek);

    }

    [Fact]
    public void GetBusinessTime()
    {
        var startDate = new DateTime(2010, 1, 4, 9, 31, 30);
        var endDate = new DateTime(2010, 1, 6, 13, 14, 16);
        var businessWeek = BusinessWeek.DefaultWeek;

        var watch = Stopwatch.StartNew();
        var time = businessWeek.GetBusinessTime(startDate, endDate);
        watch.Stop();

        Console.WriteLine("Business Time: {0}", time);
        Console.WriteLine("Time: {0} ms", watch.ElapsedMilliseconds);
        Assert.Equal(new TimeSpan(19, 42, 46), time);

        startDate = new DateTime(2010, 1, 4, 9, 31, 30);
        endDate = new DateTime(2010, 1, 30, 13, 14, 16);

        watch.Reset();
        watch.Start();
        time = businessWeek.GetBusinessTime(startDate, endDate);
        watch.Stop();

        Console.WriteLine("Business Time: {0}", time);
        Console.WriteLine("Time: {0} ms", watch.ElapsedMilliseconds);
        Assert.Equal(new TimeSpan(6, 15, 28, 30), time);

        startDate = new DateTime(2010, 1, 4, 9, 31, 30);
        endDate = new DateTime(2010, 6, 30, 13, 14, 16);

        watch.Reset();
        watch.Start();
        time = businessWeek.GetBusinessTime(startDate, endDate);
        watch.Stop();

        Console.WriteLine("Business Time: {0}", time);
        Console.WriteLine("Time: {0} ms", watch.ElapsedMilliseconds);
        Assert.Equal(new TimeSpan(42, 11, 42, 46), time);
    }

    [Fact]
    public void GetBusinessDate()
    {
        var startDate = new DateTime(2010, 1, 1);
        var endDate = new DateTime(2010, 1, 4, 11, 0, 0);
        var businessWeek = BusinessWeek.DefaultWeek;

        var watch = Stopwatch.StartNew();
        var resultDate = businessWeek.GetBusinessEndDate(startDate, TimeSpan.FromHours(10));
        watch.Stop();
        Console.WriteLine("Business Date: {0}", resultDate);
        Console.WriteLine("Time: {0} ms", watch.ElapsedMilliseconds);

        Assert.Equal(endDate, resultDate);

        startDate = new DateTime(2010, 1, 4, 9, 31, 30);

        watch.Reset();
        watch.Start();
        resultDate = businessWeek.GetBusinessEndDate(startDate, new TimeSpan(60, 10, 15, 28));
        watch.Stop();

        Console.WriteLine("Business Date: {0}", resultDate);
        Console.WriteLine("Time: {0} ms", watch.ElapsedMilliseconds);
    }

    [Fact]
    public void ThridShift()
    {
        var businessWeek = new BusinessWeek();
        //day 1
        businessWeek.BusinessDays.Add(new BusinessDay(DayOfWeek.Sunday, TimeSpan.FromHours(22), TimeSpan.FromHours(24)));
        businessWeek.BusinessDays.Add(new BusinessDay(DayOfWeek.Monday, TimeSpan.Zero, TimeSpan.FromHours(6)));
        //day 2
        businessWeek.BusinessDays.Add(new BusinessDay(DayOfWeek.Monday, TimeSpan.FromHours(22), TimeSpan.FromHours(24)));
        businessWeek.BusinessDays.Add(new BusinessDay(DayOfWeek.Tuesday, TimeSpan.Zero, TimeSpan.FromHours(6)));
        //day 3
        businessWeek.BusinessDays.Add(new BusinessDay(DayOfWeek.Tuesday, TimeSpan.FromHours(22), TimeSpan.FromHours(24)));
        businessWeek.BusinessDays.Add(new BusinessDay(DayOfWeek.Wednesday, TimeSpan.Zero, TimeSpan.FromHours(6)));
        //day 4
        businessWeek.BusinessDays.Add(new BusinessDay(DayOfWeek.Wednesday, TimeSpan.FromHours(22), TimeSpan.FromHours(24)));
        businessWeek.BusinessDays.Add(new BusinessDay(DayOfWeek.Thursday, TimeSpan.Zero, TimeSpan.FromHours(6)));
        //day 5
        businessWeek.BusinessDays.Add(new BusinessDay(DayOfWeek.Thursday, TimeSpan.FromHours(22), TimeSpan.FromHours(24)));
        businessWeek.BusinessDays.Add(new BusinessDay(DayOfWeek.Friday, TimeSpan.Zero, TimeSpan.FromHours(6)));


        var startDate = new DateTime(2010, 1, 3, 22, 0, 0);
        var endDate = new DateTime(2010, 1, 4, 6, 0, 0);

        var time = businessWeek.GetBusinessTime(startDate, endDate);

        Assert.Equal(8, time.TotalHours);

        startDate = new DateTime(2010, 1, 2, 0, 0, 0);
        endDate = new DateTime(2010, 1, 9, 0, 0, 0);

        time = businessWeek.GetBusinessTime(startDate, endDate);

        Assert.Equal(40, time.TotalHours);
    }
}
