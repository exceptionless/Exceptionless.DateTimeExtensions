using System;
using System.Collections.Generic;
using System.Linq;
using Exceptionless.DateTimeExtensions.FormatParsers;
using Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;

namespace Exceptionless.DateTimeExtensions {
    public class DateTimeRange : IEquatable<DateTimeRange>, IComparable<DateTimeRange> {
        public const string DefaultSeparator = " - ";
        public static DateTimeRange Empty = new DateTimeRange(DateTime.MinValue, DateTime.MinValue);

        public DateTimeRange(DateTime start, DateTime end) {
            Start = start < end ? start : end;
            End = end > start ? end : start;
        }

        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }

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
            return new DateTimeRange(Start.Add(timeSpan), End.Add(timeSpan));
        }

        public DateTimeRange Subtract(TimeSpan timeSpan) {
            return new DateTimeRange(Start.Subtract(timeSpan), End.Subtract(timeSpan));
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

        public static DateTimeRange Parse(string range) {
            return Parse(range, DateTime.Now);
        }

        public static DateTimeRange Parse(string content, DateTime now) {
            foreach (var parser in FormatParsers) {
                var range = parser.Parse(content, now);
                if (range != null)
                    return range;
            }

            return null;
        }
    }
}