using System;

namespace Exceptionless.DateTimeExtensions {
    public static class DateTimeExtensions {
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
            var age = GetAge(fromDate);
            if (Math.Abs(age.TotalMinutes) <= 1d)
                return age.TotalSeconds > 0 ? "Just now" : "Right now";

            if (age.TotalSeconds > 0)
                return age.ToString(1) + " ago";

            return age.ToString(1) + " from now";
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

        /// <summary>
        /// Adjust the DateTime so the time is 1 millisecond before the next day.
        /// </summary>
        /// <param name="dateTime">The DateTime to adjust.</param>
        /// <returns>A DateTime that is 1 millisecond before the next day.</returns>
        public static DateTime ToEndOfDay(this DateTime dateTime) {
            return dateTime.Date // convert to just a date with out time
                .AddDays(1) // add one day so its tomorrow
                .Subtract(TimeSpan.FromMilliseconds(1)); // subtract 1 ms
        }

        public static DateTime ChangeMillisecond(this DateTime date, int millisecond) {
            if (millisecond < 0 || millisecond > 59)
                throw new ArgumentException("Value must be between 0 and 999.", "millisecond");

            return date.AddMilliseconds(millisecond - date.Millisecond);
        }

        public static DateTime ChangeSecond(this DateTime date, int second) {
            if (second < 0 || second > 59)
                throw new ArgumentException("Value must be between 0 and 59.", "second");

            return date.AddSeconds(second - date.Second);
        }

        public static DateTime ChangeMinute(this DateTime date, int minute) {
            if (minute < 0 || minute > 59)
                throw new ArgumentException("Value must be between 0 and 59.", "minute");

            return date.AddMinutes(minute - date.Minute);
        }

        public static DateTime ChangeHour(this DateTime date, int hour) {
            if (hour < 0 || hour > 23)
                throw new ArgumentException("Value must be between 0 and 23.", "hour");

            return date.AddHours(hour - date.Hour);
        }

        public static DateTime ChangeDay(this DateTime date, int day) {
            if (day < 1 || day > 31)
                throw new ArgumentException("Value must be between 1 and 31.", "day");

            if (day > DateTime.DaysInMonth(date.Year, date.Month))
                throw new ArgumentException("Value must be a valid day.", "day");

            return date.AddDays(day - date.Day);
        }

        public static DateTime ChangeMonth(this DateTime date, int month) {
            if (month < 1 || month > 12)
                throw new ArgumentException("Value must be between 1 and 12.", "month");

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
            return date.StartOfSecond().AddSeconds(1).SubtractMilliseconds(1);
        }

        public static DateTime StartOfMinute(this DateTime date) {
            return date.Floor(TimeSpan.FromMinutes(1));
        }

        public static DateTime EndOfMinute(this DateTime date) {
            return date.StartOfMinute().AddMinutes(1).SubtractMilliseconds(1);
        }

        public static DateTime StartOfHour(this DateTime date) {
            return date.Floor(TimeSpan.FromHours(1));
        }

        public static DateTime EndOfHour(this DateTime date) {
            return date.StartOfHour().AddHours(1).SubtractMilliseconds(1);
        }

        public static DateTime EndOfDay(this DateTime date) {
            return date.Date.AddDays(1).SubtractMilliseconds(1);
        }

        public static DateTime StartOfDay(this DateTime date) {
            return date.Date;
        }

        public static DateTime StartOfWeek(this DateTime date, DayOfWeek startOfWeek = DayOfWeek.Sunday) {
            int diff = date.DayOfWeek - startOfWeek;
            if (diff < 0)
                diff += 7;

            return date.Date.AddDays(-1 * diff);
        }

        public static DateTime EndOfWeek(this DateTime date, DayOfWeek startOfWeek = DayOfWeek.Sunday) {
            return date.StartOfWeek(startOfWeek).AddWeeks(1).SubtractMilliseconds(1);
        }

        public static DateTime StartOfMonth(this DateTime date) {
            return date.Date.AddDays(1 - date.Date.Day);
        }

        public static DateTime EndOfMonth(this DateTime date) {
            return date.StartOfMonth().AddMonths(1).SubtractMilliseconds(1);
        }

        public static DateTime StartOfYear(this DateTime date) {
            return date.Date.AddDays(1 - date.Date.Day).AddMonths(1 - date.Date.Month);
        }

        public static DateTime EndOfYear(this DateTime date) {
            return date.StartOfYear().AddYears(1).SubtractMilliseconds(1);
        }

        public static DateTime Round(this DateTime date, TimeSpan roundingInterval, MidpointRounding roundingType = MidpointRounding.ToEven) {
            long ticks = (date.Ticks + (roundingInterval.Ticks / 2) + 1) / roundingInterval.Ticks;
            return new DateTime(ticks * roundingInterval.Ticks, date.Kind);
        }

        public static DateTime Floor(this DateTime date, TimeSpan roundingInterval) {
            long ticks = (date.Ticks / roundingInterval.Ticks);
            return new DateTime(ticks * roundingInterval.Ticks, date.Kind);
        }

        public static DateTime Ceiling(this DateTime date, TimeSpan roundingInterval) {
            long ticks = (date.Ticks + roundingInterval.Ticks - 1) / roundingInterval.Ticks;
            return new DateTime(ticks * roundingInterval.Ticks, date.Kind);
        }

        public static DateTime NextSecond(this DateTime date) {
            return date.AddSeconds(1);
        }

        public static DateTime LastSecond(this DateTime date) {
            return date.SubtractSeconds(1);
        }

        public static DateTime NextMinute(this DateTime date) {
            return date.AddMinutes(1);
        }

        public static DateTime LastMinute(this DateTime date) {
            return date.SubtractMinutes(1);
        }

        public static DateTime NextHour(this DateTime date) {
            return date.AddHours(1);
        }

        public static DateTime LastHour(this DateTime date) {
            return date.SubtractHours(1);
        }

        public static DateTime NextDay(this DateTime date) {
            return date.AddDays(1);
        }

        public static DateTime LastDay(this DateTime date) {
            return date.SubtractDays(1);
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
                throw new ArgumentException("Value cannot be less than 0.", "value");

            return date.AddTicks(value * -1);
        }

        public static DateTime SubtractMilliseconds(this DateTime date, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", "value");

            return date.AddMilliseconds(value * -1);
        }

        public static DateTime SubtractSeconds(this DateTime date, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", "value");

            return date.AddSeconds(value * -1);
        }

        public static DateTime SubtractMinutes(this DateTime date, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", "value");

            return date.AddMinutes(value * -1);
        }

        public static DateTime SubtractHours(this DateTime date, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", "value");

            return date.AddHours(value * -1);
        }

        public static DateTime SubtractDays(this DateTime date, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", "value");

            return date.AddDays(value * -1);
        }

        public static DateTime AddWeeks(this DateTime date, double value) {
            return date.AddDays(value * 7);
        }

        public static DateTime SubtractWeeks(this DateTime date, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", "value");

            return date.AddWeeks(value * -1);
        }

        public static DateTime SubtractMonths(this DateTime date, int months) {
            if (months < 0)
                throw new ArgumentException("Months cannot be less than 0.", "months");

            return date.AddMonths(months * -1);
        }

        public static DateTime SubtractYears(this DateTime date, int value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", "value");

            return date.AddYears(value * -1);
        }
    }
}
