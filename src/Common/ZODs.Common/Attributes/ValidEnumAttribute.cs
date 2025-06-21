using ZODs.Common.Extensions;
using System.ComponentModel.DataAnnotations;

namespace ZODs.Common.Attributes
{
    /// <summary>
    /// Checks whether a given integral value, or its name as a string, exists in a specified enumeration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValidEnumAttribute : ValidationAttribute
    {
        private readonly string defaultErrorMessage = "Invalid value for enum {0}.";

        private Type enumType;

        public ValidEnumAttribute(Type enumType = null, string errorMessage = "")
        {
            if (enumType?.IsEnum == false)
            {
                throw new ArgumentException($"{nameof(enumType)} must be an enum type.");
            }

            this.enumType = enumType;
            this.ErrorMessage = errorMessage;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (this.enumType == null)
            {
                // Get type of property being validated
                this.enumType = validationContext.ObjectType?.GetProperty(validationContext.DisplayName)?.PropertyType;

                if (this.enumType?.IsNullable() == true)
                {
                    // If property is nullable, get underlying type, so we can check if it's valid enum
                    this.enumType = Nullable.GetUnderlyingType(this.enumType);
                }
            }

            if (this.enumType?.IsEnum == true && !Enum.IsDefined(this.enumType, value))
            {
                if (string.IsNullOrWhiteSpace(this.ErrorMessage))
                {
                    this.ErrorMessage = string.Format(this.defaultErrorMessage, this.enumType.Name);
                }

                return new ValidationResult(this.ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
