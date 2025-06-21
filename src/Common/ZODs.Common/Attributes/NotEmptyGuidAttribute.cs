using System.ComponentModel.DataAnnotations;

namespace ZODs.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class NotEmptyGuidAttribute : ValidationAttribute
    {
        /// <summary>
        /// Error message to display if the validation fails.
        /// </summary>
        private const string DefaultErrorMessage = "The GUID field must not be empty.";

        /// <summary>
        /// Initializes a new instance of the <see cref="NotEmptyGuidAttribute"/> class.
        /// </summary>
        public NotEmptyGuidAttribute() : base(DefaultErrorMessage)
        {
        }

        /// <summary>
        /// Validates that the GUID is not empty.
        /// </summary>
        /// <param name="value">The value of the GUID field.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        /// <returns>An instance of the <see cref="ValidationResult"/> class.</returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is Guid guidValue && guidValue == Guid.Empty)
            {
                return new ValidationResult(DefaultErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}