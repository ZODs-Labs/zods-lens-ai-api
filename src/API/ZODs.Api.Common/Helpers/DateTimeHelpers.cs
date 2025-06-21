namespace ZODs.Api.Common.Helpers
{
    public static class DateTimeHelpers
    {
        /// <summary>
        /// Converts a number of seconds from now into a Unix timestamp.
        /// </summary>
        /// <param name="secondsFromNow">The number of seconds from the current time.</param>
        /// <returns>The Unix timestamp representing the future time.</returns>
        public static long ConvertSecondsFromNowToUnixTimestamp(long secondsFromNow)
        {
            var futureTimeUtc = DateTime.UtcNow.AddSeconds(secondsFromNow);
            var futureTimeUnix = new DateTimeOffset(futureTimeUtc).ToUnixTimeSeconds();
            return futureTimeUnix;
        }

        public static long ConvertUtcToUnixTimestamp(this DateTime utcDateTime)
        {
            // Unix timestamp starts from January 1, 1970
            DateTime unixStartTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Calculate the elapsed time between the given UTC DateTime and Unix start time
            TimeSpan elapsedTime = utcDateTime - unixStartTime;

            // Convert the elapsed time to seconds and return
            return (long)elapsedTime.TotalSeconds;
        }
    }
}
