using System;

namespace Exceptionless.DateTimeExtensions {
    public static class DateTimeExtensions {
        public static bool IsBefore(this DateTime date, DateTime value) {
            return date < value;
        }

        public static bool IsBeforeOrEqual(this DateTime date, DateTime value) {
            return date <= value;
        }

        public static bool IsAfter(this DateTime date, DateTime value) {
            return date > value;
        }

        public static bool IsAfterOrEqual(this DateTime date, DateTime value) {
            return date >= value;
        }

        public static DateTime SafeAdd(this DateTime date, TimeSpan value) {
            if (date.Ticks + value.Ticks < DateTime.MinValue.Ticks)
                return DateTime.MinValue;

            if (date.Ticks + value.Ticks > DateTime.MaxValue.Ticks)
                return DateTime.MaxValue;

            return date.Add(value);
        }

        public static DateTime SafeSubtract(this DateTime date, TimeSpan value) {
            if (date.Ticks - value.Ticks < DateTime.MinValue.Ticks)
                return DateTime.MinValue;

            if (date.Ticks - value.Ticks > DateTime.MaxValue.Ticks)
                return DateTime.MaxValue;

            return date.Subtract(value);
        }

        public static string ToApproximateAgeString(this DateTime fromDate) {
            var isFuture = fromDate > DateTime.Now;
            var age = isFuture ? GetAge(DateTime.Now, fromDate) : GetAge(fromDate);
            if (age.TotalMinutes <= 1d)
                return age.TotalSeconds > 0 ? "Just now" : "Right now";

            return isFuture ? $"{age.ToString(1)} from now" : $"{age.ToString(1)} ago";
        }

        public static string ToAgeString(this DateTime fromDate) {
            return ToAgeString(fromDate, DateTime.Now, 0);
        }

        public static string ToAgeString(this DateTime fromDate, int maxSpans) {
            return ToAgeString(fromDate, DateTime.Now, maxSpans);
        }

        public static string ToAgeString(this DateTime fromDate, int maxSpans, bool shortForm) {
            return ToAgeString(fromDate, DateTime.Now, maxSpans, shortForm);
        }

        public static string ToAgeString(this DateTime fromDate, DateTime toDate, int maxSpans) {
            var age = GetAge(fromDate, toDate);
            return age.ToString(maxSpans, false);
        }

        public static string ToAgeString(this DateTime fromDate, DateTime toDate, int maxSpans, bool shortForm) {
            var age = GetAge(fromDate, toDate);
            return age.ToString(maxSpans, shortForm);
        }

        public static AgeSpan GetAge(this DateTime fromDate) {
            return GetAge(fromDate, DateTime.Now);
        }

        public static AgeSpan GetAge(this DateTime fromDate, DateTime toDate) {
            return new AgeSpan(toDate - fromDate);
        }

        public static int ToEpoch(this DateTime fromDate) {
            var utc = (fromDate.ToUniversalTime().Ticks - EPOCH_TICKS) / TimeSpan.TicksPerSecond;
            return Convert.ToInt32(utc);
        }

        public static int ToEpochOffset(this DateTime date, int timestamp) {
            return timestamp - date.ToEpoch();
        }

        public static int ToEpoch(this DateTime date, int offset) {
            return offset + date.ToEpoch();
        }

        private const long EPOCH_TICKS = 621355968000000000;

        public static DateTime ToDateTime(this int secondsSinceEpoch) {
            return new DateTime(EPOCH_TICKS + (secondsSinceEpoch * TimeSpan.TicksPerSecond));
        }

        public static DateTime ToDateTime(this double milliSecondsSinceEpoch) {
            return new DateTime(EPOCH_TICKS + ((long)milliSecondsSinceEpoch * TimeSpan.TicksPerMillisecond));
        }

        public static DateTime ChangeMillisecond(this DateTime date, int millisecond) {
            if (millisecond < 0 || millisecond > 59)
                throw new ArgumentException("Value must be between 0 and 999.", nameof(millisecond));

            return date.AddMilliseconds(millisecond - date.Millisecond);
        }

        public static DateTime ChangeSecond(this DateTime date, int second) {
            if (second < 0 || second > 59)
                throw new ArgumentException("Value must be between 0 and 59.", nameof(second));

            return date.AddSeconds(second - date.Second);
        }

        public static DateTime ChangeMinute(this DateTime date, int minute) {
            if (minute < 0 || minute > 59)
                throw new ArgumentException("Value must be between 0 and 59.", nameof(minute));

            return date.AddMinutes(minute - date.Minute);
        }

        public static DateTime ChangeHour(this DateTime date, int hour) {
            if (hour < 0 || hour > 23)
                throw new ArgumentException("Value must be between 0 and 23.", nameof(hour));

            return date.AddHours(hour - date.Hour);
        }

        public static DateTime ChangeDay(this DateTime date, int day) {
            if (day < 1 || day > 31)
                throw new ArgumentException("Value must be between 1 and 31.", nameof(day));

            if (day > DateTime.DaysInMonth(date.Year, date.Month))
                throw new ArgumentException("Value must be a valid source.", nameof(day));

            return date.AddDays(day - date.Day);
        }

        public static DateTime ChangeMonth(this DateTime date, int month) {
            if (month < 1 || month > 12)
                throw new ArgumentException("Value must be between 1 and 12.", nameof(month));

            return date.AddMonths(month - date.Month);
        }

        public static DateTime ChangeYear(this DateTime date, int year) {
            return date.AddYears(year - date.Year);
        }

        public static DateTime Change(this DateTime date, int? year = null, int? month = null, int? day = null, int? hour = null, int? minute = null, int? second = null) {
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

        public static DateTime StartOfSecond(this DateTime date) {
            return date.Floor(TimeSpan.FromSeconds(1));
        }

        public static DateTime EndOfSecond(this DateTime date) {
            var value = date.StartOfSecond().SafeAdd(TimeSpan.FromSeconds(1));
            if (value == DateTime.MaxValue)
                return value;

            return value.SubtractMilliseconds(1);
        }

        public static DateTime StartOfMinute(this DateTime date) {
            return date.Floor(TimeSpan.FromMinutes(1));
        }

        public static DateTime EndOfMinute(this DateTime date) {
            var value = date.StartOfMinute().SafeAdd(TimeSpan.FromMinutes(1));
            if (value == DateTime.MaxValue)
                return value;

            return value.SubtractMilliseconds(1);
        }

        public static DateTime StartOfHour(this DateTime date) {
            return date.Floor(TimeSpan.FromHours(1));
        }

        public static DateTime EndOfHour(this DateTime date) {
            var value = date.StartOfHour().SafeAdd(TimeSpan.FromHours(1));
            if (value == DateTime.MaxValue)
                return value;

            return value.SubtractMilliseconds(1);
        }

        public static DateTime StartOfDay(this DateTime date) {
            return date.Floor(TimeSpan.FromDays(1));
        }

        public static DateTime EndOfDay(this DateTime date) {
            var value = date.StartOfDay().SafeAdd(TimeSpan.FromDays(1));
            if (value == DateTime.MaxValue)
                return value;

            return value.SubtractMilliseconds(1);
        }

        public static DateTime StartOfWeek(this DateTime date, DayOfWeek startOfWeek = DayOfWeek.Sunday) {
            int diff = date.DayOfWeek - startOfWeek;
            if (diff < 0)
                diff += 7;

            return date.StartOfDay().SafeSubtract(TimeSpan.FromDays(diff));
        }

        public static DateTime EndOfWeek(this DateTime date, DayOfWeek startOfWeek = DayOfWeek.Sunday) {
            var value = date.StartOfWeek(startOfWeek).AddWeeks(1);
            if (value == DateTime.MaxValue)
                return value;

            return value.SubtractMilliseconds(1);
        }

        public static DateTime StartOfMonth(this DateTime date) {
            return date.StartOfDay().SafeSubtract(TimeSpan.FromDays(date.Date.Day - 1));
        }

        public static DateTime EndOfMonth(this DateTime date) {
            var value = date.StartOfMonth().AddMonths(1);
            if (value == DateTime.MaxValue)
                return value;

            return value.SubtractMilliseconds(1);
        }

        public static DateTime StartOfYear(this DateTime date) {
            return date.StartOfMonth().SubtractMonths(date.Date.Month - 1);
        }

        public static DateTime EndOfYear(this DateTime date) {
            var value = date.StartOfYear().AddYears(1);
            if (value == DateTime.MaxValue)
                return value;

            return value.SubtractMilliseconds(1);
        }

        public static DateTime Floor(this DateTime date, TimeSpan interval) {
            return date.AddTicks(-(date.Ticks % interval.Ticks));
        }

        public static DateTime Ceiling(this DateTime date, TimeSpan interval) {
            return date.AddTicks(interval.Ticks - (date.Ticks % interval.Ticks));
        }

        public static DateTime Round(this DateTime date, TimeSpan roundingInterval) {
            var halfIntervalTicks = ((roundingInterval.Ticks + 1) >> 1);
            return date.AddTicks(halfIntervalTicks - ((date.Ticks + halfIntervalTicks) % roundingInterval.Ticks));
        }

        public static DateTime NextSecond(this DateTime date) {
            return date.SafeAdd(TimeSpan.FromSeconds(1));
        }

        public static DateTime LastSecond(this DateTime date) {
            return date.SafeSubtract(TimeSpan.FromSeconds(1));
        }

        public static DateTime NextMinute(this DateTime date) {
            return date.SafeAdd(TimeSpan.FromMinutes(1));
        }

        public static DateTime LastMinute(this DateTime date) {
            return date.SafeSubtract(TimeSpan.FromMinutes(1));
        }

        public static DateTime NextHour(this DateTime date) {
            return date.SafeAdd(TimeSpan.FromHours(1));
        }

        public static DateTime LastHour(this DateTime date) {
            return date.SafeSubtract(TimeSpan.FromHours(1));
        }

        public static DateTime NextDay(this DateTime date) {
            return date.SafeAdd(TimeSpan.FromDays(1));
        }

        public static DateTime LastDay(this DateTime date) {
            return date.SafeSubtract(TimeSpan.FromDays(1));
        }

        public static DateTime NextWeek(this DateTime date) {
            return date.AddWeeks(1);
        }

        public static DateTime LastWeek(this DateTime date) {
            return date.SubtractWeeks(1);
        }

        public static DateTime NextMonth(this DateTime date) {
            return date.AddMonths(1);
        }

        public static DateTime LastMonth(this DateTime date) {
            return date.SubtractMonths(1);
        }

        public static DateTime NextYear(this DateTime date) {
            return date.AddYears(1);
        }

        public static DateTime LastYear(this DateTime date) {
            return date.SubtractYears(1);
        }

        public static DateTime SubtractTicks(this DateTime date, long value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", nameof(value));

            return date.SafeSubtract(TimeSpan.FromTicks(value));
        }

        public static DateTime SubtractMilliseconds(this DateTime date, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", nameof(value));

            return date.SafeSubtract(TimeSpan.FromMilliseconds(value));
        }

        public static DateTime SubtractSeconds(this DateTime date, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", nameof(value));

            return date.SafeSubtract(TimeSpan.FromSeconds(value));
        }

        public static DateTime SubtractMinutes(this DateTime date, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", nameof(value));

            return date.SafeSubtract(TimeSpan.FromMinutes(value));
        }

        public static DateTime SubtractHours(this DateTime date, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", nameof(value));

            return date.SafeSubtract(TimeSpan.FromHours(value));
        }

        public static DateTime SubtractDays(this DateTime date, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", nameof(value));

            return date.SafeSubtract(TimeSpan.FromDays(value));
        }

        public static DateTime AddWeeks(this DateTime date, double value) {
            return date.SafeAdd(TimeSpan.FromDays(value * 7));
        }

        public static DateTime SubtractWeeks(this DateTime date, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", nameof(value));

            return date.SafeSubtract(TimeSpan.FromDays(value * 7));
        }

        public static DateTime SubtractMonths(this DateTime date, int months) {
            if (months < 0)
                throw new ArgumentException("Months cannot be less than 0.", nameof(months));

            return date.AddMonths(months * -1);
        }

        public static DateTime SubtractYears(this DateTime date, int value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", nameof(value));

            return date.AddYears(value * -1);
        }

        public static bool Intersects(this DateTime source, DateTime start, DateTime end) {
            return source >= start && source <= end;
        }

        public static bool IntersectsSecond(this DateTime source, DateTime date) {
            return source.Intersects(date.StartOfSecond(), date.EndOfSecond());
        }

        public static bool IntersectsMinute(this DateTime source, DateTime date) {
            return source.Intersects(date.StartOfMinute(), date.EndOfMinute());
        }

        public static bool IntersectsHour(this DateTime source, DateTime date) {
            return source.Intersects(date.StartOfHour(), date.EndOfHour());
        }

        public static bool IntersectsDay(this DateTime source, DateTime date) {
            return source.Intersects(date.StartOfDay(), date.EndOfDay());
        }

        public static bool IntersectsMonth(this DateTime source, DateTime date) {
            return source.Intersects(date.StartOfMonth(), date.EndOfMonth());
        }

        public static bool IntersectsYear(this DateTime source, DateTime date) {
            return source.Intersects(date.StartOfYear(), date.EndOfYear());
        }
    }
}