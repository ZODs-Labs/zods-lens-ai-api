namespace ZODs.Common.Extensions
{
    public static class CommonExtensions
    {
        /// <summary>
        /// Checks whether a <see cref="Type"/> is nullable or not.
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <returns>True if type is nullable, otherwise False.</returns>
        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static int TryParseIntOrZero(this string value)
        {
            return int.TryParse(value, out var result) ? result : 0;
        }

        public static int TryParseIntOrZero(this object value)
        {
            return int.TryParse(value?.ToString(), out var result) ? result : 0;
        }
    }
}
