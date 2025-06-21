using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ZODs.Common.Attributes
{
    /// <summary>
    /// Validates that the sort by property is a property of the target type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class IsValidSortByOfAttribute : ValidationAttribute
    {
        private readonly Type _targetType;

        public IsValidSortByOfAttribute(Type targetType)
        {
            _targetType = targetType;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is not string sortProperty)
            {
                return new ValidationResult("This attribute is only applicable to string properties.");
            }

            PropertyInfo property = _targetType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p => string.Equals(p.Name, sortProperty, StringComparison.OrdinalIgnoreCase));

            if (property == null)
            {
                return new ValidationResult($"The sort by property '{sortProperty}' does not exist in {_targetType.Name}.");
            }

            // Set the actual name of the sort by property.
            var objectInstance = validationContext.ObjectInstance;
            var type = objectInstance.GetType();
            PropertyInfo propertyToSet = type.GetProperty(validationContext.MemberName);
            propertyToSet?.SetValue(objectInstance, property.Name, null);

            return ValidationResult.Success;
        }
    }
}
