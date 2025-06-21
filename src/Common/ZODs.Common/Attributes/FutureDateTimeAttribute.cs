using ZODs.Common.Enums;
using ZODs.Common.Extensions;
using System.ComponentModel.DataAnnotations;

namespace ZODs.Common.Attributes
{
    /// <summary>
    /// Specifies that a DateTime field must be in future.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class FutureDateTimeAttribute : ValidationAttribute
    {
        public FutureDateTimeAttribute(string? httpMethod = null, bool allowCurrentDateTime = false)
        {
            this.HttpMethod = httpMethod?.ToUpper() ?? "GET";
            this.AllowCurrentDateTime = allowCurrentDateTime;
        }

        /// <summary>
        /// Gets or sets the HTTP method for which this validation attribute should be applied.
        /// </summary>
        public string HttpMethod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current date/time should be allowed.
        /// </summary>
        public bool AllowCurrentDateTime { get; set; }

        protected override ValidationResult? IsValid(object? objValue, ValidationContext validationContext)
        {
            if (objValue is DateTime dateTime)
            {
                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);

                var utcDateTime = DateTime.UtcNow.StartOf(DateTimeUnit.Minute);

                if ((this.AllowCurrentDateTime && dateTime < utcDateTime) ||
                    (!this.AllowCurrentDateTime && dateTime <= utcDateTime))
                {
                    var propertyInfo = validationContext.ObjectType.GetProperty(validationContext?.MemberName ?? string.Empty);

                    return new ValidationResult($"{propertyInfo!.Name} should be in the future.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
