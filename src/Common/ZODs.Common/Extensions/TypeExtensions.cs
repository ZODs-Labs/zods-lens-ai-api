using System.Reflection;

namespace ZODs.Common.Extensions
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Checks if the given Type contains a property that matches the provided property name.
        /// </summary>
        /// <param name="type">The Type to check.</param>
        /// <param name="propName">The property name to search for.</param>
        /// <param name="bindingFlags">Optional. Specifies flags that control the search for the property.</param>
        /// <returns>True if the property is found, false otherwise.</returns>
        public static bool HasProperty(this Type type, string propName, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            // Check for nulls to prevent NullReferenceException
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "The type parameter should not be null.");
            }

            if (string.IsNullOrWhiteSpace(propName))
            {
                return true;
            }

            var hasProperty = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Any(p => string.Equals(p.Name, propName, StringComparison.OrdinalIgnoreCase));

            // If the property is not null, it exists in the given type.
            return hasProperty;
        }
    }
}
