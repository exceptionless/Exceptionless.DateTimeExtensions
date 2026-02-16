using System.Diagnostics;

namespace Exceptionless.DateTimeExtensions;

/// <summary>
/// A record defining a business day.
/// </summary>
[DebuggerDisplay("DayOfWeek={DayOfWeek}, StartTime={StartTime}, EndTime={EndTime}")]
public record BusinessDay
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessDay"/> record with default 9am–5pm hours.
    /// </summary>
    /// <param name="dayOfWeek">The day of week this business day represents.</param>
    public BusinessDay(DayOfWeek dayOfWeek) : this(dayOfWeek, TimeSpan.FromHours(9) /* 9am */, TimeSpan.FromHours(17) /* 5pm */) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessDay"/> record.
    /// </summary>
    /// <param name="dayOfWeek">The day of week this business day represents.</param>
    /// <param name="startTime">The start time of the business day.</param>
    /// <param name="endTime">The end time of the business day.</param>
    public BusinessDay(DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(startTime.TotalDays, 1.0, nameof(startTime));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(endTime.TotalDays, 1.0, nameof(endTime));
        if (endTime <= startTime)
            throw new ArgumentOutOfRangeException(nameof(endTime), endTime, "The endTime argument must be greater than startTime.");

        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
    }

    /// <summary>
    /// Gets the day of week this business day represents.
    /// </summary>
    /// <value>The day of week.</value>
    public DayOfWeek DayOfWeek { get; init; }

    /// <summary>
    /// Gets the start time of the business day.
    /// </summary>
    /// <value>The start time of the business day.</value>
    public TimeSpan StartTime { get; init; }

    /// <summary>
    /// Gets the end time of the business day.
    /// </summary>
    /// <value>The end time of the business day.</value>
    public TimeSpan EndTime { get; init; }

    /// <summary>
    /// Determines whether the specified date falls in the business day.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <returns>
    /// 	<c>true</c> if the specified date falls in the business day; otherwise, <c>false</c>.
    /// </returns>
    public bool IsBusinessDay(DateTime date) =>
        date.DayOfWeek == DayOfWeek && date.TimeOfDay >= StartTime && date.TimeOfDay <= EndTime;
}
