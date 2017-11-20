using System;

namespace Exceptionless.DateTimeExtensions {
    public static class DateTimeOffsetExtensions {
        public static bool IsBefore(this DateTimeOffset date, DateTimeOffset value) {
            return date < value;
        }

        public static bool IsBeforeOrEqual(this DateTimeOffset date, DateTimeOffset value) {
            return date <= value;
        }

        public static bool IsAfter(this DateTimeOffset date, DateTimeOffset value) {
            return date > value;
        }

        public static bool IsAfterOrEqual(this DateTimeOffset date, DateTimeOffset value) {
            return date >= value;
        }

        public static DateTimeOffset SafeAdd(this DateTimeOffset date, TimeSpan value) {
            if (date.Ticks + value.Ticks < DateTime.MinValue.Ticks)
                return DateTime.MinValue;

            if (date.Ticks + value.Ticks > DateTime.MaxValue.Ticks)
                return DateTime.MaxValue;

            return date.Add(value);
        }

        public static DateTimeOffset SafeSubtract(this DateTimeOffset date, TimeSpan value) {
            if (date.Ticks - value.Ticks < DateTime.MinValue.Ticks)
                return DateTime.MinValue;

            if (date.Ticks - value.Ticks > DateTime.MaxValue.Ticks)
                return DateTime.MaxValue;

            return date.Subtract(value);
        }

        public static string ToApproximateAgeString(this DateTimeOffset fromDate) {
            bool isFuture = fromDate > DateTimeOffset.Now;
            var age = isFuture ? GetAge(DateTimeOffset.Now, fromDate) : GetAge(fromDate);
            if (age.TotalMinutes <= 1d)
                return age.TotalSeconds > 0 ? "Just now" : "Right now";

            return isFuture ? $"{age.ToString(1)} from now" : $"{age.ToString(1)} ago";
        }

        public static string ToAgeString(this DateTimeOffset fromDate) {
            return ToAgeString(fromDate, DateTimeOffset.Now, 0);
        }

        public static string ToAgeString(this DateTimeOffset fromDate, int maxSpans) {
            return ToAgeString(fromDate, DateTimeOffset.Now, maxSpans);
        }

        public static string ToAgeString(this DateTimeOffset fromDate, int maxSpans, bool shortForm) {
            return ToAgeString(fromDate, DateTimeOffset.Now, maxSpans, shortForm);
        }

        public static string ToAgeString(this DateTimeOffset fromDate, DateTimeOffset toDate, int maxSpans) {
            var age = GetAge(fromDate, toDate);
            return age.ToString(maxSpans, false);
        }

        public static string ToAgeString(this DateTimeOffset fromDate, DateTimeOffset toDate, int maxSpans, bool shortForm) {
            var age = GetAge(fromDate, toDate);
            return age.ToString(maxSpans, shortForm);
        }

        public static AgeSpan GetAge(this DateTimeOffset fromDate) {
            return GetAge(fromDate, DateTimeOffset.Now);
        }

        public static AgeSpan GetAge(this DateTimeOffset fromDate, DateTimeOffset toDate) {
            return new AgeSpan(toDate - fromDate);
        }

        public static int ToEpoch(this DateTimeOffset fromDate) {
            long utc = (fromDate.ToUniversalTime().Ticks - EPOCH_TICKS) / TimeSpan.TicksPerSecond;
            return Convert.ToInt32(utc);
        }

        public static int ToEpochOffset(this DateTimeOffset date, int timestamp) {
            return timestamp - date.ToEpoch();
        }

        public static int ToEpoch(this DateTimeOffset date, int offset) {
            return offset + date.ToEpoch();
        }

        private const long EPOCH_TICKS = 621355968000000000;

        public static DateTimeOffset ToDateTimeOffset(this int secondsSinceEpoch, TimeSpan offset) {
            return new DateTimeOffset(EPOCH_TICKS + (secondsSinceEpoch * TimeSpan.TicksPerSecond), offset);
        }

        public static DateTimeOffset ToDateTimeOffset(this double milliSecondsSinceEpoch, TimeSpan offset) {
            return new DateTimeOffset(EPOCH_TICKS + ((long)milliSecondsSinceEpoch * TimeSpan.TicksPerMillisecond), offset);
        }

        public static DateTimeOffset ChangeMillisecond(this DateTimeOffset date, int millisecond) {
            if (millisecond < 0 || millisecond > 59)
                throw new ArgumentException("Value must be between 0 and 999.", nameof(millisecond));

            return date.AddMilliseconds(millisecond - date.Millisecond);
        }

        public static DateTimeOffset ChangeSecond(this DateTimeOffset date, int second) {
            if (second < 0 || second > 59)
                throw new ArgumentException("Value must be between 0 and 59.", nameof(second));

            return date.AddSeconds(second - date.Second);
        }

        public static DateTimeOffset ChangeMinute(this DateTimeOffset date, int minute) {
            if (minute < 0 || minute > 59)
                throw new ArgumentException("Value must be between 0 and 59.", nameof(minute));

            return date.AddMinutes(minute - date.Minute);
        }

        public static DateTimeOffset ChangeHour(this DateTimeOffset date, int hour) {
            if (hour < 0 || hour > 23)
                throw new ArgumentException("Value must be between 0 and 23.", nameof(hour));

            return date.AddHours(hour - date.Hour);
        }

        public static DateTimeOffset ChangeDay(this DateTimeOffset date, int day) {
            if (day < 1 || day > 31)
                throw new ArgumentException("Value must be between 1 and 31.", nameof(day));

            if (day > DateTime.DaysInMonth(date.Year, date.Month))
                throw new ArgumentException("Value must be a valid source.", nameof(day));

            return date.AddDays(day - date.Day);
        }

        public static DateTimeOffset ChangeMonth(this DateTimeOffset date, int month) {
            if (month < 1 || month > 12)
                throw new ArgumentException("Value must be between 1 and 12.", nameof(month));

            return date.AddMonths(month - date.Month);
        }

        public static DateTimeOffset ChangeYear(this DateTimeOffset date, int year) {
            return date.AddYears(year - date.Year);
        }

        /// <summary>
        /// Changes an existing Offset without modifying the time
        /// </summary>
        public static DateTimeOffset ChangeOffset(this DateTimeOffset date, TimeSpan offset) {
            return new DateTimeOffset(date.DateTime, offset);
        }

        public static DateTimeOffset Change(this DateTimeOffset date, int? year = null, int? month = null, int? day = null, int? hour = null, int? minute = null, int? second = null) {
            var result = date;

            if (year.HasValue)
                result = result.ChangeYear(year.Value);
            if (month.HasValue)
                result = result.ChangeMonth(month.Value);
            if (day.HasValue)
                result = result.ChangeDay(day.Value);
            if (hour.HasValue)
                result = result.ChangeHour(hour.Value);
            if (minute.HasValue)
                result = result.ChangeMinute(minute.Value);
            if (second.HasValue)
                result = result.ChangeSecond(second.Value);

            return result;
        }

        public static DateTimeOffset StartOfSecond(this DateTimeOffset date) {
            return date.Floor(TimeSpan.FromSeconds(1));
        }

        public static DateTimeOffset EndOfSecond(this DateTimeOffset date) {
            var value = date.StartOfSecond().SafeAdd(TimeSpan.FromSeconds(1));
            if (value == DateTimeOffset.MaxValue)
                return value;

            return value.SubtractMilliseconds(1);
        }

        public static DateTimeOffset StartOfMinute(this DateTimeOffset date) {
            return date.Floor(TimeSpan.FromMinutes(1));
        }

        public static DateTimeOffset EndOfMinute(this DateTimeOffset date) {
            var value = date.StartOfMinute().SafeAdd(TimeSpan.FromMinutes(1));
            if (value == DateTimeOffset.MaxValue)
                return value;

            return value.SubtractMilliseconds(1);
        }

        public static DateTimeOffset StartOfHour(this DateTimeOffset date) {
            return date.Floor(TimeSpan.FromHours(1));
        }

        public static DateTimeOffset EndOfHour(this DateTimeOffset date) {
            var value = date.StartOfHour().SafeAdd(TimeSpan.FromHours(1));
            if (value == DateTimeOffset.MaxValue)
                return value;

            return value.SubtractMilliseconds(1);
        }

        public static DateTimeOffset StartOfDay(this DateTimeOffset date) {
            return date.Floor(TimeSpan.FromDays(1));
        }

        public static DateTimeOffset EndOfDay(this DateTimeOffset date) {
            var value = date.StartOfDay().SafeAdd(TimeSpan.FromDays(1));
            if (value == DateTimeOffset.MaxValue)
                return value;

            return value.SubtractMilliseconds(1);
        }

        public static DateTimeOffset StartOfWeek(this DateTimeOffset date, DayOfWeek startOfWeek = DayOfWeek.Sunday) {
            int diff = date.DayOfWeek - startOfWeek;
            if (diff < 0)
                diff += 7;

            return date.StartOfDay().SafeSubtract(TimeSpan.FromDays(diff));
        }

        public static DateTimeOffset EndOfWeek(this DateTimeOffset date, DayOfWeek startOfWeek = DayOfWeek.Sunday) {
            var value = date.StartOfWeek(startOfWeek).AddWeeks(1);
            if (value == DateTimeOffset.MaxValue)
                return value;

            return value.SubtractMilliseconds(1);
        }

        public static DateTimeOffset StartOfMonth(this DateTimeOffset date) {
            return date.StartOfDay().SafeSubtract(TimeSpan.FromDays(date.Date.Day - 1));
        }

        public static DateTimeOffset EndOfMonth(this DateTimeOffset date) {
            var value = date.StartOfMonth().AddMonths(1);
            if (value == DateTimeOffset.MaxValue)
                return value;

            return value.SubtractMilliseconds(1);
        }

        public static DateTimeOffset StartOfYear(this DateTimeOffset date) {
            return date.StartOfMonth().SubtractMonths(date.Date.Month - 1);
        }

        public static DateTimeOffset EndOfYear(this DateTimeOffset date) {
            var value = date.StartOfYear().AddYears(1);
            if (value == DateTimeOffset.MaxValue)
                return value;

            return value.SubtractMilliseconds(1);
        }

        public static DateTimeOffset Floor(this DateTimeOffset date, TimeSpan interval) {
            return date.AddTicks(-(date.Ticks % interval.Ticks));
        }

        public static DateTimeOffset Ceiling(this DateTimeOffset date, TimeSpan interval) {
            return date.AddTicks(interval.Ticks - (date.Ticks % interval.Ticks));
        }

        public static DateTimeOffset Round(this DateTimeOffset date, TimeSpan roundingInterval) {
            long halfIntervalTicks = ((roundingInterval.Ticks + 1) >> 1);
            return date.AddTicks(halfIntervalTicks - ((date.Ticks + halfIntervalTicks) % roundingInterval.Ticks));
        }

        public static DateTimeOffset NextSecond(this DateTimeOffset date) {
            return date.SafeAdd(TimeSpan.FromSeconds(1));
        }

        public static DateTimeOffset LastSecond(this DateTimeOffset date) {
            return date.SafeSubtract(TimeSpan.FromSeconds(1));
        }

        public static DateTimeOffset NextMinute(this DateTimeOffset date) {
            return date.SafeAdd(TimeSpan.FromMinutes(1));
        }

        public static DateTimeOffset LastMinute(this DateTimeOffset date) {
            return date.SafeSubtract(TimeSpan.FromMinutes(1));
        }

        public static DateTimeOffset NextHour(this DateTimeOffset date) {
            return date.SafeAdd(TimeSpan.FromHours(1));
        }

        public static DateTimeOffset LastHour(this DateTimeOffset date) {
            return date.SafeSubtract(TimeSpan.FromHours(1));
        }

        public static DateTimeOffset NextDay(this DateTimeOffset date) {
            return date.SafeAdd(TimeSpan.FromDays(1));
        }

        public static DateTimeOffset LastDay(this DateTimeOffset date) {
            return date.SafeSubtract(TimeSpan.FromDays(1));
        }

        public static DateTimeOffset NextWeek(this DateTimeOffset date) {
            return date.AddWeeks(1);
        }

        public static DateTimeOffset LastWeek(this DateTimeOffset date) {
            return date.SubtractWeeks(1);
        }

        public static DateTimeOffset NextMonth(this DateTimeOffset date) {
            return date.AddMonths(1);
        }

        public static DateTimeOffset LastMonth(this DateTimeOffset date) {
            return date.SubtractMonths(1);
        }

        public static DateTimeOffset NextYear(this DateTimeOffset date) {
            return date.AddYears(1);
        }

        public static DateTimeOffset LastYear(this DateTimeOffset date) {
            return date.SubtractYears(1);
        }

        public static DateTimeOffset SubtractTicks(this DateTimeOffset date, long value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", nameof(value));

            return date.SafeSubtract(TimeSpan.FromTicks(value));
        }

        public static DateTimeOffset SubtractMilliseconds(this DateTimeOffset date, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", nameof(value));

            return date.SafeSubtract(TimeSpan.FromMilliseconds(value));
        }

        public static DateTimeOffset SubtractSeconds(this DateTimeOffset date, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", nameof(value));

            return date.SafeSubtract(TimeSpan.FromSeconds(value));
        }

        public static DateTimeOffset SubtractMinutes(this DateTimeOffset date, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", nameof(value));

            return date.SafeSubtract(TimeSpan.FromMinutes(value));
        }

        public static DateTimeOffset SubtractHours(this DateTimeOffset date, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", nameof(value));

            return date.SafeSubtract(TimeSpan.FromHours(value));
        }

        public static DateTimeOffset SubtractDays(this DateTimeOffset date, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", nameof(value));

            return date.SafeSubtract(TimeSpan.FromDays(value));
        }

        public static DateTimeOffset AddWeeks(this DateTimeOffset date, double value) {
            return date.SafeAdd(TimeSpan.FromDays(value * 7));
        }

        public static DateTimeOffset SubtractWeeks(this DateTimeOffset date, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", nameof(value));

            return date.SafeSubtract(TimeSpan.FromDays(value * 7));
        }

        public static DateTimeOffset SubtractMonths(this DateTimeOffset date, int months) {
            if (months < 0)
                throw new ArgumentException("Months cannot be less than 0.", nameof(months));

            return date.AddMonths(months * -1);
        }

        public static DateTimeOffset SubtractYears(this DateTimeOffset date, int value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", nameof(value));

            return date.AddYears(value * -1);
        }

        public static bool Intersects(this DateTimeOffset source, DateTimeOffset start, DateTimeOffset end) {
            return source >= start && source <= end;
        }

        public static bool IntersectsSecond(this DateTimeOffset source, DateTimeOffset date) {
            return source.Intersects(date.StartOfSecond(), date.EndOfSecond());
        }

        public static bool IntersectsMinute(this DateTimeOffset source, DateTimeOffset date) {
            return source.Intersects(date.StartOfMinute(), date.EndOfMinute());
        }

        public static bool IntersectsHour(this DateTimeOffset source, DateTimeOffset date) {
            return source.Intersects(date.StartOfHour(), date.EndOfHour());
        }

        public static bool IntersectsDay(this DateTimeOffset source, DateTimeOffset date) {
            return source.Intersects(date.StartOfDay(), date.EndOfDay());
        }

        public static bool IntersectsMonth(this DateTimeOffset source, DateTimeOffset date) {
            return source.Intersects(date.StartOfMonth(), date.EndOfMonth());
        }

        public static bool IntersectsYear(this DateTimeOffset source, DateTimeOffset date) {
            return source.Intersects(date.StartOfYear(), date.EndOfYear());
        }
    }
}