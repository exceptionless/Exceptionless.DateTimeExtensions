using System;

namespace Exceptionless.DateTimeExtensions {
    public static class DateTimeOffsetExtensions {
        public static string ToApproximateAgeString(this DateTimeOffset fromDate) {
            var age = GetAge(fromDate);
            if (Math.Abs(age.TotalMinutes) <= 1d)
                return age.TotalSeconds > 0 ? "Just now" : "Right now";

            if (age.TotalSeconds > 0)
                return age.ToString(1) + " ago";

            return age.ToString(1) + " from now";
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
            var utc = (fromDate.ToUniversalTime().Ticks - EPOCH_TICKS) / TimeSpan.TicksPerSecond;
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

        public static DateTimeOffset ChangeDay(this DateTimeOffset dateTime, int day) {
            return dateTime.AddDays(day - dateTime.Date.Day);
        }

        public static DateTimeOffset ChangeYear(this DateTimeOffset dateTime, int year) {
            return dateTime.AddYears(year - dateTime.Date.Year);
        }

        public static DateTimeOffset ChangeMonth(this DateTimeOffset dateTime, int month) {
            return dateTime.AddMonths(month - dateTime.Date.Month);
        }

        public static DateTimeOffset ToStartOfDay(this DateTimeOffset dateTime) {
            return new DateTimeOffset(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, dateTime.Offset);
        }

        public static DateTimeOffset ToEndOfDay(this DateTimeOffset dateTime)
        {
            return dateTime.ToStartOfDay()
                .AddDays(1)
                .Subtract(TimeSpan.FromMilliseconds(1));
        }

        public static DateTimeOffset ToStartOfYear(this DateTimeOffset dateTime) {
            return dateTime.ToStartOfDay().AddDays(1 - dateTime.ToStartOfDay().Day).AddMonths(1 - dateTime.Date.Month);
        }

        public static DateTimeOffset ToEndOfYear(this DateTimeOffset dateTime) {
            return dateTime.ToStartOfYear().AddYears(1).AddSeconds(-1);
        }

        public static DateTimeOffset ToStartOfMonth(this DateTimeOffset dateTime) {
            return dateTime.ToStartOfDay().AddDays(1 - dateTime.ToStartOfDay().Day);
        }

        public static DateTimeOffset ToEndOfMonth(this DateTimeOffset dateTime) {
            return dateTime.ToStartOfMonth().AddMonths(1).AddSeconds(-1);
        }

        public static DateTimeOffset Round(this DateTimeOffset datetime, TimeSpan roundingInterval, MidpointRounding roundingType = MidpointRounding.ToEven) {
            return new DateTimeOffset((datetime.UtcDateTime - DateTimeOffset.MinValue).Round(roundingInterval, roundingType).Ticks, datetime.Offset);
        }

        public static DateTimeOffset Floor(this DateTimeOffset datetime, TimeSpan roundingInterval) {
            return new DateTimeOffset((datetime.UtcDateTime - DateTimeOffset.MinValue).Floor(roundingInterval).Ticks, datetime.Offset);
        }

        public static DateTimeOffset Ceiling(this DateTimeOffset datetime, TimeSpan roundingInterval) {
            return new DateTimeOffset((datetime.UtcDateTime - DateTimeOffset.MinValue).Ceiling(roundingInterval).Ticks, datetime.Offset);
        }

        #region Subtract Extensions

        public static DateTimeOffset SubtractDays(this DateTimeOffset date, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", "value");

            return date.AddDays(value * -1);
        }

        public static DateTimeOffset SubtractHours(this DateTimeOffset date, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", "value");

            return date.AddHours(value * -1);
        }

        public static DateTimeOffset SubtractMilliseconds(this DateTimeOffset date, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", "value");

            return date.AddMilliseconds(value * -1);
        }

        public static DateTimeOffset SubtractMinutes(this DateTimeOffset date, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", "value");

            return date.AddMinutes(value * -1);
        }

        public static DateTimeOffset SubtractMonths(this DateTimeOffset date, int months) {
            if (months < 0)
                throw new ArgumentException("Months cannot be less than 0.", "months");

            return date.AddMonths(months * -1);
        }

        public static DateTimeOffset SubtractSeconds(this DateTimeOffset date, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", "value");

            return date.AddSeconds(value * -1);
        }

        public static DateTimeOffset SubtractTicks(this DateTimeOffset date, long value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", "value");

            return date.AddTicks(value * -1);
        }

        public static DateTimeOffset SubtractYears(this DateTimeOffset date, int value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", "value");

            return date.AddYears(value * -1);
        }

        #endregion
    }
}
