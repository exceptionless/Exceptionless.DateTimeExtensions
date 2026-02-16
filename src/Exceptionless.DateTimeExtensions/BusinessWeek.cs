using System.Collections.Frozen;

namespace Exceptionless.DateTimeExtensions;

/// <summary>
/// A class representing a business week.
/// </summary>
public class BusinessWeek
{
    private readonly FrozenDictionary<DayOfWeek, IReadOnlyList<BusinessDay>> _dayTree;

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessWeek"/> class with default Mon–Fri 9am–5pm business days.
    /// </summary>
    public BusinessWeek() : this([
        new(DayOfWeek.Monday), new(DayOfWeek.Tuesday), new(DayOfWeek.Wednesday),
        new(DayOfWeek.Thursday), new(DayOfWeek.Friday)
    ])
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessWeek"/> class with the specified business days.
    /// </summary>
    /// <param name="businessDays">The business days that define the week.</param>
    /// <exception cref="ArgumentNullException"><paramref name="businessDays"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="businessDays"/> is empty.</exception>
    public BusinessWeek(IEnumerable<BusinessDay> businessDays)
    {
        ArgumentNullException.ThrowIfNull(businessDays);
        BusinessDays = businessDays.ToList().AsReadOnly();
        if (BusinessDays.Count == 0)
            throw new ArgumentException("Must have at least one business day.", nameof(businessDays));

        _dayTree = BusinessDays
            .OrderBy(b => b.DayOfWeek).ThenBy(b => b.StartTime)
            .GroupBy(b => b.DayOfWeek)
            .ToFrozenDictionary(g => g.Key, g => (IReadOnlyList<BusinessDay>)g.ToList().AsReadOnly());
    }

    /// <summary>
    /// Gets the business days for the week.
    /// </summary>
    /// <value>The business days for the week.</value>
    public IReadOnlyList<BusinessDay> BusinessDays { get; }

    /// <summary>
    /// Gets the default BusinessWeek (Mon–Fri 9am–5pm).
    /// </summary>
    public static BusinessWeek DefaultWeek { get; } = new();

    /// <summary>
    /// Determines whether the specified date falls on a business day.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <returns>
    /// 	<c>true</c> if the specified date falls on a business day; otherwise, <c>false</c>.
    /// </returns>
    public bool IsBusinessDay(DateTime date)
    {
        return BusinessDays.Any(day => day.IsBusinessDay(date));
    }

    /// <summary>
    /// Gets the business time between the start date and end date.
    /// </summary>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <returns>
    /// A TimeSpan of the amount of business time between the start and end date.
    /// </returns>
    /// <remarks>
    /// Business time is calculated by adding only the time that falls inside the business day range.
    /// If all the time between the start and end date fall outside the business day, the time will be zero.
    /// </remarks>
    public TimeSpan GetBusinessTime(DateTime startDate, DateTime endDate)
    {
        Validate(true);

        var businessTime = TimeSpan.Zero;
        var workingDate = startDate;

        while (workingDate < endDate)
        {
            // get start date
            if (!NextBusinessDay(workingDate, out var businessStart, out var businessDay))
                break;

            // business start after end date
            if (businessStart > endDate)
                break;

            if (businessDay is null)
                break;

            var timeToEndOfDay = businessDay.EndTime.Subtract(businessStart.TimeOfDay);
            var businessEnd = businessStart.SafeAdd(timeToEndOfDay);

            if (endDate <= businessEnd)
            {
                timeToEndOfDay = endDate.TimeOfDay.Subtract(businessStart.TimeOfDay);
                businessTime = businessTime.Add(timeToEndOfDay);
                return businessTime;
            }

            // still more time left, use business end date
            businessTime = businessTime.Add(timeToEndOfDay);
            workingDate = businessEnd;
        }

        return businessTime;
    }

    /// <summary>
    /// Gets the business end date using the specified time.
    /// </summary>
    /// <param name="startDate">The start date.</param>
    /// <param name="businessTime">The business time.</param>
    /// <returns>The business end date.</returns>
    public DateTime GetBusinessEndDate(DateTime startDate, TimeSpan businessTime)
    {
        Validate(true);

        var endDate = startDate;
        var remainingTime = businessTime;

        while (remainingTime > TimeSpan.Zero)
        {
            // get start date
            if (!NextBusinessDay(endDate, out var businessStart, out var businessDay))
                break;

            if (businessDay is null)
                break;

            var timeForDay = businessDay.EndTime.Subtract(businessStart.TimeOfDay);
            if (remainingTime <= timeForDay)
                return businessStart.SafeAdd(remainingTime);

            // still more time left
            remainingTime = remainingTime.Subtract(timeForDay);
            endDate = businessStart.SafeAdd(timeForDay);
        }

        return endDate;
    }

    /// <summary>
    /// Validates the business week.
    /// </summary>
    /// <param name="throwException">if set to <c>true</c> throw exception if invalid.</param>
    /// <returns><c>true</c> if valid; otherwise <c>false</c>.</returns>
    protected virtual bool Validate(bool throwException)
    {
        if (BusinessDays.Count > 0)
            return true;

        if (throwException)
            throw new InvalidOperationException("The BusinessDays property must have at least one BusinessDay.");

        return false;
    }

    internal bool NextBusinessDay(DateTime startDate, out DateTime nextDate, out BusinessDay? businessDay)
    {
        nextDate = startDate;
        businessDay = null;

        // loop no more than 7 times
        for (int x = 0; x < 7; x++)
        {
            var dayOfWeek = nextDate.DayOfWeek;

            if (!_dayTree.TryGetValue(dayOfWeek, out var businessDays))
            {
                // no business days on this day of the week
                nextDate = nextDate.AddDays(1).Date;
                continue;
            }

            foreach (var day in businessDays)
            {
                var timeOfDay = nextDate.TimeOfDay;

                if (timeOfDay >= day.StartTime && timeOfDay < day.EndTime)
                {
                    // working date in range
                    businessDay = day;
                    return true;
                }

                // past this business day, try other for this day
                if (timeOfDay >= day.StartTime)
                    continue;

                // move to start time
                businessDay = day;
                nextDate = nextDate.Date.SafeAdd(day.StartTime);

                return true;
            }

            // next day
            nextDate = nextDate.AddDays(1).Date;
        }

        // should never reach here
        return false;
    }
}
