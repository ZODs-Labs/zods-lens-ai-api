using ZODs.Common.Enums;

namespace ZODs.Common.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Sets the date/time to start of specific DateTime unit.
        /// </summary>
        /// <param name="dateTime">DateTime to change the time.</param>
        /// <param name="granularity">DateTime unit to specify granularity.</param>
        /// <returns>DateTime object.</returns>
        public static DateTime StartOf(this DateTime dateTime, DateTimeUnit granularity)
        {
            return granularity switch
            {
                DateTimeUnit.Year => new DateTime(dateTime.Year, 1, 1),
                DateTimeUnit.Month => new DateTime(dateTime.Year, dateTime.Month, 1),
                DateTimeUnit.Day => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day),
                DateTimeUnit.Hour => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0),
                DateTimeUnit.Minute => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0),
                DateTimeUnit.Second => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second),
                _ => dateTime,
            };
        }

        /// <summary>
        /// Converts <see cref="DateTime"/> from one time zone to another time zone.
        /// </summary>
        /// <param name="dateTime"><see cref="DateTime"/> to convert.</param>
        /// <param name="timeZone">Current time zone.</param>
        /// <param name="toTimeZone">Time zone to convert in.</param>
        /// <param name="isIanaTimeZone">Flag indicates whether the current time zone is in IANA format.</param>
        /// <returns><see cref="DateTime"/> with new time zone.</returns>
        public static DateTime ConvertTimeZone(this DateTime dateTime, string timeZone, string toTimeZone, bool isIanaTimeZone = true)
        {
            ArgumentNullException.ThrowIfNull(timeZone);
            ArgumentNullException.ThrowIfNull(toTimeZone);

            if (!isIanaTimeZone)
            {
                if (TimeZoneInfo.TryConvertWindowsIdToIanaId(timeZone, out string ianaTimeZoneId))
                {
                    timeZone = ianaTimeZoneId;
                }
                else
                {
                    throw new InvalidTimeZoneException();
                }
            }

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);

            var toTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(toTimeZone);

            var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified), timeZoneInfo);

            return toTimeZone.Equals("UTC") ? utcDateTime : TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, toTimeZoneInfo);
        }

        /// <summary>
        /// Converts string representation of date to Datetime object.
        /// </summary>
        /// <param name="value">String representation of date.</param>
        /// <returns>Date. Current UTC date is returned in case that conversion fails.</returns>
        public static DateTime ToDateTime(this string value)
        {
            if (DateTime.TryParse(value, out var date))
            {
                return date;
            }

            return DateTime.UtcNow;
        }

        /// <summary>
        /// Check if date and time are in the future.
        /// By default, granularity is limited to minutes and date/time are compared to the current date/time in the UTC time zone.
        /// </summary>
        /// <param name="dateTime">DateTime to compare.</param>
        /// <returns>A flag indicating whether the date and time are in the future.</returns>
        public static bool IsFutureDateTime(this DateTime dateTime) =>
                dateTime.StartOf(DateTimeUnit.Minute) > DateTime.UtcNow.StartOf(DateTimeUnit.Minute);

        /// <summary>
        /// Checks if the date is the same as today's UTC date.
        /// </summary>
        /// <param name="dateTime"><see cref="DateTime"/> to compare.</param>
        /// <returns>True if the date is the same as today's UTC date, otherwise False.</returns>
        public static bool IsTodayUTC(this DateTime dateTime) => dateTime.Date.Equals(DateTime.UtcNow.Date);

        /// <summary>
        /// Checks if the DateTime is within the specified range.
        /// </summary>
        /// <param name="dateTime">DateTime to check.</param>
        /// <param name="start">Range start.</param>
        /// <param name="end">Range end.</param>
        /// <returns>True if DateTime is withing the specified range, False if not.</returns>
        public static bool IsWithinRange(this DateTime dateTime, DateTime start, DateTime end)
        {
            return dateTime >= start && dateTime <= end;
        }

        /// <summary>
        /// Converts DateTime to Unix timestamp.
        /// </summary>
        /// <param name="dateTime">DateTime to convert.</param>
        /// <returns>Unix timestamp in milliseconds.</returns>
        public static long ToUnixTimestamp(this DateTime dateTime) => (long)(dateTime - CommonConstants.EpochDateTime).TotalMilliseconds;
    }
}
