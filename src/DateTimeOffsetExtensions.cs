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

        public static int ToEpochOffset(this DateTimeOffset dateTime, int timestamp) {
            return timestamp - dateTime.ToEpoch();
        }

        public static int ToEpoch(this DateTimeOffset dateTime, int offset) {
            return offset + dateTime.ToEpoch();
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

        public static DateTimeOffset StartOfSecond(this DateTimeOffset dateTime) {
            return dateTime.Floor(TimeSpan.FromSeconds(1));
        }

        public static DateTimeOffset EndOfSecond(this DateTimeOffset dateTime) {
            return dateTime.StartOfSecond().AddSeconds(1).SubtractMilliseconds(1);
        }

        public static DateTimeOffset StartOfMinute(this DateTimeOffset dateTime) {
            return dateTime.Floor(TimeSpan.FromMinutes(1));
        }

        public static DateTimeOffset EndOfMinute(this DateTimeOffset dateTime) {
            return dateTime.StartOfMinute().AddMinutes(1).SubtractMilliseconds(1);
        }

        public static DateTimeOffset StartOfHour(this DateTimeOffset dateTime) {
            return dateTime.Floor(TimeSpan.FromHours(1));
        }

        public static DateTimeOffset EndOfHour(this DateTimeOffset dateTime) {
            return dateTime.StartOfHour().AddHours(1).SubtractMilliseconds(1);
        }

        public static DateTimeOffset StartOfDay(this DateTimeOffset dateTime) {
            return dateTime.Floor(TimeSpan.FromDays(1));
        }

        public static DateTimeOffset EndOfDay(this DateTimeOffset dateTime) {
            return dateTime.StartOfDay().AddDays(1).Subtract(TimeSpan.FromMilliseconds(1));
        }

        public static DateTimeOffset StartOfWeek(this DateTimeOffset dateTime, DayOfWeek startOfWeek = DayOfWeek.Sunday) {
            int diff = dateTime.DayOfWeek - startOfWeek;
            if (diff < 0)
                diff += 7;

            return dateTime.Date.AddDays(-1 * diff);
        }

        public static DateTimeOffset EndOfWeek(this DateTimeOffset dateTime, DayOfWeek startOfWeek = DayOfWeek.Sunday) {
            return dateTime.StartOfWeek(startOfWeek).AddWeeks(1).SubtractMilliseconds(1);
        }

        public static DateTimeOffset StartOfMonth(this DateTimeOffset dateTime) {
            return dateTime.StartOfDay().AddDays(1 - dateTime.StartOfDay().Day);
        }

        public static DateTimeOffset EndOfMonth(this DateTimeOffset dateTime) {
            return dateTime.StartOfMonth().AddMonths(1).AddSeconds(-1);
        }

        public static DateTimeOffset StartOfYear(this DateTimeOffset dateTime) {
            return dateTime.StartOfDay().AddDays(1 - dateTime.StartOfDay().Day).AddMonths(1 - dateTime.Date.Month);
        }

        public static DateTimeOffset EndOfYear(this DateTimeOffset dateTime) {
            return dateTime.StartOfYear().AddYears(1).AddSeconds(-1);
        }

        public static DateTimeOffset Floor(this DateTimeOffset dateTime, TimeSpan interval) {
            return dateTime.AddTicks(-(dateTime.Ticks % interval.Ticks));
        }

        public static DateTimeOffset Ceiling(this DateTimeOffset dateTime, TimeSpan interval) {
            return dateTime.AddTicks(interval.Ticks - (dateTime.Ticks % interval.Ticks));
        }

        public static DateTimeOffset Round(this DateTimeOffset dateTime, TimeSpan roundingInterval) {
            var halfIntervalTicks = ((roundingInterval.Ticks + 1) >> 1);
            return dateTime.AddTicks(halfIntervalTicks - ((dateTime.Ticks + halfIntervalTicks) % roundingInterval.Ticks));
        }

        public static DateTimeOffset NextSecond(this DateTimeOffset dateTime) {
            return dateTime.AddSeconds(1);
        }

        public static DateTimeOffset LastSecond(this DateTimeOffset dateTime) {
            return dateTime.SubtractSeconds(1);
        }

        public static DateTimeOffset NextMinute(this DateTimeOffset dateTime) {
            return dateTime.AddMinutes(1);
        }

        public static DateTimeOffset LastMinute(this DateTimeOffset dateTime) {
            return dateTime.SubtractMinutes(1);
        }

        public static DateTimeOffset NextHour(this DateTimeOffset dateTime) {
            return dateTime.AddHours(1);
        }

        public static DateTimeOffset LastHour(this DateTimeOffset dateTime) {
            return dateTime.SubtractHours(1);
        }

        public static DateTimeOffset NextDay(this DateTimeOffset dateTime) {
            return dateTime.AddDays(1);
        }

        public static DateTimeOffset LastDay(this DateTimeOffset dateTime) {
            return dateTime.SubtractDays(1);
        }

        public static DateTimeOffset NextWeek(this DateTimeOffset dateTime) {
            return dateTime.AddWeeks(1);
        }

        public static DateTimeOffset LastWeek(this DateTimeOffset dateTime) {
            return dateTime.SubtractWeeks(1);
        }

        public static DateTimeOffset NextMonth(this DateTimeOffset dateTime) {
            return dateTime.AddMonths(1);
        }

        public static DateTimeOffset LastMonth(this DateTimeOffset dateTime) {
            return dateTime.SubtractMonths(1);
        }

        public static DateTimeOffset NextYear(this DateTimeOffset dateTime) {
            return dateTime.AddYears(1);
        }

        public static DateTimeOffset LastYear(this DateTimeOffset dateTime) {
            return dateTime.SubtractYears(1);
        }

        public static DateTimeOffset SubtractTicks(this DateTimeOffset dateTime, long value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", nameof(value));

            return dateTime.AddTicks(value * -1);
        }

        public static DateTimeOffset SubtractMilliseconds(this DateTimeOffset dateTime, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", nameof(value));

            return dateTime.AddMilliseconds(value * -1);
        }

        public static DateTimeOffset SubtractSeconds(this DateTimeOffset dateTime, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", nameof(value));

            return dateTime.AddSeconds(value * -1);
        }

        public static DateTimeOffset SubtractMinutes(this DateTimeOffset dateTime, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", nameof(value));

            return dateTime.AddMinutes(value * -1);
        }

        public static DateTimeOffset SubtractHours(this DateTimeOffset dateTime, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", nameof(value));

            return dateTime.AddHours(value * -1);
        }

        public static DateTimeOffset SubtractDays(this DateTimeOffset dateTime, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", nameof(value));

            return dateTime.AddDays(value * -1);
        }

        public static DateTimeOffset AddWeeks(this DateTimeOffset dateTime, double value) {
            return dateTime.AddDays(value * 7);
        }

        public static DateTimeOffset SubtractWeeks(this DateTimeOffset dateTime, double value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", nameof(value));

            return dateTime.AddWeeks(value * -1);
        }

        public static DateTimeOffset SubtractMonths(this DateTimeOffset dateTime, int months) {
            if (months < 0)
                throw new ArgumentException("Months cannot be less than 0.", nameof(months));

            return dateTime.AddMonths(months * -1);
        }

        public static DateTimeOffset SubtractYears(this DateTimeOffset dateTime, int value) {
            if (value < 0)
                throw new ArgumentException("Value cannot be less than 0.", nameof(value));

            return dateTime.AddYears(value * -1);
        }

        public static bool Intersects(this DateTimeOffset source, DateTimeOffset start, DateTimeOffset end) {
            return source >= start && source <= end;
        }

        public static bool IntersectsSecond(this DateTimeOffset source, DateTimeOffset dateTime) {
            return source.Intersects(dateTime.StartOfSecond(), dateTime.EndOfSecond());
        }

        public static bool IntersectsMinute(this DateTimeOffset source, DateTimeOffset dateTime) {
            return source.Intersects(dateTime.StartOfMinute(), dateTime.EndOfMinute());
        }

        public static bool IntersectsHour(this DateTimeOffset source, DateTimeOffset dateTime) {
            return source.Intersects(dateTime.StartOfHour(), dateTime.EndOfHour());
        }

        public static bool IntersectsDay(this DateTimeOffset source, DateTimeOffset dateTime) {
            return source.Intersects(dateTime.StartOfDay(), dateTime.EndOfDay());
        }

        public static bool IntersectsMonth(this DateTimeOffset source, DateTimeOffset dateTime) {
            return source.Intersects(dateTime.StartOfMonth(), dateTime.EndOfMonth());
        }

        public static bool IntersectsYear(this DateTimeOffset source, DateTimeOffset dateTime) {
            return source.Intersects(dateTime.StartOfYear(), dateTime.EndOfYear());
        }
    }
}