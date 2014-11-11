using System;
using System.Collections.Generic;
using System.Linq;
using Exceptionless.DateTimeExtensions.FormatParsers;
using Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;

namespace Exceptionless.DateTimeExtensions {
    public class DateTimeRange : IEquatable<DateTimeRange>, IComparable<DateTimeRange> {
        public static DateTimeRange Empty = new DateTimeRange(DateTime.MinValue, DateTime.MinValue);
        public static DateTimeRange All = new DateTimeRange(DateTime.MinValue, DateTime.MaxValue);

        public const string DefaultSeparator = " - ";
        private readonly DateTime _start;
        private readonly DateTime _end;

        public DateTimeRange(DateTime start, DateTime end) {
            _start = start < end ? start : end;
            _end = end > start ? end : start;
        }

        public DateTime Start { get { return _start; } }
        public DateTime End { get { return _end; } }

        public DateTime UtcStart { get { return Start.ToUniversalTime(); } }
        public DateTime UtcEnd { get { return End.ToUniversalTime(); } }

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
            return new DateTimeRange(Start.SafeAdd(timeSpan), End.SafeAdd(timeSpan));
        }

        public DateTimeRange Subtract(TimeSpan timeSpan) {
            return new DateTimeRange(Start.SafeSubtract(timeSpan), End.SafeSubtract(timeSpan));
        }

        public bool Intersects(DateTimeRange other) {
            return Contains(other.Start) || Contains(other.End);
        }

        public DateTimeRange Intersection(DateTimeRange other) {
            DateTime greatestStart = Start > other.Start ? Start : other.Start;
            DateTime smallestEnd = End < other.End ? End : other.End;

            if (greatestStart > smallestEnd)
                return null;

            return new DateTimeRange(greatestStart, smallestEnd);
        }

        public bool Contains(DateTime time) {
            return time >= Start && time <= End;
        }

        private static List<IFormatParser> _formatParsers = null;

        public static List<IFormatParser> FormatParsers {
            get {
                if (_formatParsers == null) {
                    var formatParserTypes = TypeHelper.GetDerivedTypes<IFormatParser>().SortByPriority();
                    _formatParsers = new List<IFormatParser>(formatParserTypes.Select(t => Activator.CreateInstance(t) as IFormatParser));
                }

                return _formatParsers;
            }
        }

        private static List<IPartParser> _partParsers = null;

        public static List<IPartParser> PartParsers {
            get {
                if (_partParsers == null) {
                    var partParserTypes = TypeHelper.GetDerivedTypes<IPartParser>().SortByPriority();
                    _partParsers = new List<IPartParser>(partParserTypes.Select(t => Activator.CreateInstance(t) as IPartParser));
                }

                return _partParsers;
            }
        }

        public static DateTimeRange Parse(string content) {
            return Parse(content, DateTime.Now);
        }

        public static DateTimeRange Parse(string content, DateTime now) {
            if (String.IsNullOrEmpty(content))
                return Empty;

            foreach (var parser in FormatParsers) {
                var range = parser.Parse(content, now);
                if (range != null)
                    return range;
            }

            return Empty;
        }
    }
}