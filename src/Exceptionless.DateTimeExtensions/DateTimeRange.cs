using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Exceptionless.DateTimeExtensions.FormatParsers;
using Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;

namespace Exceptionless.DateTimeExtensions {
    [DebuggerDisplay("{Start} {DefaultSeparator} {End}")]
    public class DateTimeRange : IEquatable<DateTimeRange>, IComparable<DateTimeRange> {
        public static readonly DateTimeRange Empty = new(DateTime.MinValue, DateTime.MaxValue);

        public const string DefaultSeparator = " - ";

        public DateTimeRange(DateTime start, DateTime end) {
            Start = start < end ? start : end;
            UtcStart = Start != DateTime.MinValue ? Start.ToUniversalTime() : Start;
            End = end > start ? end : start;
            UtcEnd = End != DateTime.MaxValue ? End.ToUniversalTime() : End;
        }

        public DateTimeRange(DateTimeOffset start, DateTimeOffset end) {
            Start = (start = start < end ? start : end).DateTime;
            UtcStart = start.UtcDateTime;
            End = (end = end > start ? end : start).DateTime;
            UtcEnd = end.UtcDateTime;
        }

        public DateTime Start { get; }
        public DateTime End { get; }

        public DateTime UtcStart { get; }
        public DateTime UtcEnd { get; }

        public static bool operator ==(DateTimeRange left, DateTimeRange right) {
            if (ReferenceEquals(left, right))
                return true;

            if (((object)left == null) || ((object)right == null))
                return false;

            return (left.Start == right.Start) && (left.End == right.End);
        }

        public static bool operator !=(DateTimeRange left, DateTimeRange right) {
            return !(left == right);
        }

        public override bool Equals(object obj) {
            if (obj == null)
                return false;

            var other = obj as DateTimeRange;
            if ((object)other == null)
                return false;

            return (Start == other.Start) && (End == other.End);
        }

        public bool Equals(DateTimeRange other) {
            if ((object)other == null)
                return false;

            return (Start == other.Start) && (End == other.End);
        }

        public override int GetHashCode() {
            return (Start.Ticks + End.Ticks).GetHashCode();
        }

        public override string ToString() {
            return ToString(DefaultSeparator);
        }

        public string ToString(string separator) {
            return Start + separator + End;
        }

        public int CompareTo(DateTimeRange other) {
            if (other == null)
                return 1;

            if (Equals(other))
                return 0;

            return Start.CompareTo(other.End);
        }

        public DateTimeRange Add(TimeSpan timeSpan) {
            var offset = Start - UtcStart;
            return new DateTimeRange(new DateTimeOffset(Start.SafeAdd(timeSpan), offset), new DateTimeOffset(End.SafeAdd(timeSpan), offset));
        }

        public DateTimeRange Subtract(TimeSpan timeSpan) {
            var offset = Start - UtcStart;
            return new DateTimeRange(new DateTimeOffset(Start.SafeSubtract(timeSpan), offset), new DateTimeOffset(End.SafeSubtract(timeSpan), offset));
        }

        public bool Intersects(DateTimeRange other) {
            return Contains(other.UtcStart) || Contains(other.UtcEnd);
        }

        public DateTimeRange Intersection(DateTimeRange other) {
            var greatestStart = Start > other.Start ? Start : other.Start;
            var smallestEnd = End < other.End ? End : other.End;

            if (greatestStart > smallestEnd)
                return null;

            return new DateTimeRange(greatestStart, smallestEnd);
        }

        public bool Contains(DateTime time) {
            if (time.Kind == DateTimeKind.Utc)
                return time >= UtcStart && time <= UtcEnd;

            return time >= Start && time <= End;
        }

        private static List<IFormatParser> _formatParsers;

        public static List<IFormatParser> FormatParsers {
            get {
                if (_formatParsers == null) {
                    var formatParserTypes = TypeHelper.GetDerivedTypes<IFormatParser>().SortByPriority();
                    _formatParsers = new List<IFormatParser>(formatParserTypes.Select(t => Activator.CreateInstance(t) as IFormatParser));
                }

                return _formatParsers;
            }
        }

        private static List<IPartParser> _partParsers;

        public static List<IPartParser> PartParsers {
            get {
                if (_partParsers == null) {
                    var partParserTypes = TypeHelper.GetDerivedTypes<IPartParser>().SortByPriority();
                    _partParsers = new List<IPartParser>(partParserTypes.Select(t => Activator.CreateInstance(t) as IPartParser));
                }

                return _partParsers;
            }
        }

        /// <summary>
        /// Parses the date range from the passed in content.
        /// </summary>
        /// <param name="content">String date range</param>
        public static DateTimeRange Parse(string content) {
            return Parse(content, DateTimeOffset.Now);
        }

        /// <summary>
        /// Parses the date range from the passed in content.
        /// </summary>
        /// <param name="content">String date range</param>
        /// <param name="relativeBaseTime">Relative dates will be base on this time.</param>
        public static DateTimeRange Parse(string content, DateTimeOffset relativeBaseTime) {
            if (String.IsNullOrEmpty(content))
                return Empty;

            foreach (var parser in FormatParsers) {
                var range = parser.Parse(content, relativeBaseTime);
                if (range != null)
                    return range;
            }

            return Empty;
        }
    }
}