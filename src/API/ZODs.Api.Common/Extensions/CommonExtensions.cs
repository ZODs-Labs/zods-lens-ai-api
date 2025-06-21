namespace ZODs.Api.Common.Extensions
{
    public static class CommonExtensions
    {
        /// <summary>
        /// Splits string if not null or empty, otherwise returns empty array.
        /// </summary>
        /// <param name="value">String to split.</param>
        /// <param name="delimiter">Delimiter.</param>
        /// <returns><see cref="IEnumerable"/>.</returns>
        public static IEnumerable<string> SplitIfNotEmpty(this string value, char delimiter = ',') => string.IsNullOrWhiteSpace(value) ? Array.Empty<string>() : value.Split(delimiter);

    }
}
