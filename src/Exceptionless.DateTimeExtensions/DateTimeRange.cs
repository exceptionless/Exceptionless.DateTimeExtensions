using System.Diagnostics;
using Exceptionless.DateTimeExtensions.FormatParsers;
using Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;

namespace Exceptionless.DateTimeExtensions;

[DebuggerDisplay("{Start} {DefaultSeparator} {End}")]
public record DateTimeRange : IComparable<DateTimeRange>
{
    public static readonly DateTimeRange Empty = new(DateTime.MinValue, DateTime.MaxValue);

    public const string DefaultSeparator = " - ";

    public DateTimeRange(DateTime start, DateTime end)
    {
        Start = start < end ? start : end;
        UtcStart = Start != DateTime.MinValue ? Start.ToUniversalTime() : Start;
        End = end > start ? end : start;
        UtcEnd = End != DateTime.MaxValue ? End.ToUniversalTime() : End;
    }

    public DateTimeRange(DateTimeOffset start, DateTimeOffset end)
    {
        Start = (start = start < end ? start : end).DateTime;
        UtcStart = start.UtcDateTime;
        End = (end = end > start ? end : start).DateTime;
        UtcEnd = end.UtcDateTime;
    }

    public DateTime Start { get; }
    public DateTime End { get; }
    public DateTime UtcStart { get; }
    public DateTime UtcEnd { get; }

    public override string ToString()
    {
        return ToString(DefaultSeparator);
    }

    public string ToString(string separator)
    {
        return $"{Start}{separator}{End}";
    }

    public int CompareTo(DateTimeRange? other)
    {
        if (other is null)
            return 1;

        if (Equals(other))
            return 0;

        return Start.CompareTo(other.End);
    }

    public DateTimeRange Add(TimeSpan timeSpan)
    {
        var offset = Start - UtcStart;
        return new DateTimeRange(new DateTimeOffset(Start.SafeAdd(timeSpan), offset), new DateTimeOffset(End.SafeAdd(timeSpan), offset));
    }

    public DateTimeRange Subtract(TimeSpan timeSpan)
    {
        var offset = Start - UtcStart;
        return new DateTimeRange(new DateTimeOffset(Start.SafeSubtract(timeSpan), offset), new DateTimeOffset(End.SafeSubtract(timeSpan), offset));
    }

    public bool Intersects(DateTimeRange other)
    {
        return Contains(other.UtcStart) || Contains(other.UtcEnd);
    }

    public DateTimeRange? Intersection(DateTimeRange other)
    {
        var greatestStart = Start > other.Start ? Start : other.Start;
        var smallestEnd = End < other.End ? End : other.End;

        if (greatestStart > smallestEnd)
            return null;

        return new DateTimeRange(greatestStart, smallestEnd);
    }

    public bool Contains(DateTime time)
    {
        if (time.Kind == DateTimeKind.Utc)
            return time >= UtcStart && time <= UtcEnd;

        return time >= Start && time <= End;
    }

    private static List<IFormatParser>? _formatParsers;

    public static IReadOnlyList<IFormatParser> FormatParsers
    {
        get
        {
            _formatParsers ??= new List<IFormatParser>(
                TypeHelper.GetDerivedTypes<IFormatParser>()
                    .SortByPriority()
                    .Select(t => (IFormatParser)Activator.CreateInstance(t)!));

            return _formatParsers;
        }
    }

    private static List<IPartParser>? _partParsers;

    public static IReadOnlyList<IPartParser> PartParsers
    {
        get
        {
            _partParsers ??= new List<IPartParser>(
                TypeHelper.GetDerivedTypes<IPartParser>()
                    .SortByPriority()
                    .Select(t => (IPartParser)Activator.CreateInstance(t)!));

            return _partParsers;
        }
    }

    /// <summary>
    /// Parses the date range from the passed in content.
    /// </summary>
    /// <param name="content">String date range</param>
    public static DateTimeRange Parse(string content)
    {
        return Parse(content, DateTimeOffset.Now);
    }

    /// <summary>
    /// Parses the date range from the passed in content.
    /// </summary>
    /// <param name="content">String date range</param>
    /// <param name="relativeBaseTime">Relative dates will be base on this time.</param>
    public static DateTimeRange Parse(string content, DateTimeOffset relativeBaseTime)
    {
        if (String.IsNullOrEmpty(content))
            return Empty;

        foreach (var parser in FormatParsers)
        {
            var range = parser.Parse(content, relativeBaseTime);
            if (range is not null)
                return range;
        }

        return Empty;
    }
}
