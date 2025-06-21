using System.ComponentModel.DataAnnotations;

namespace ZODs.Common.Attributes
{
    /// <summary>
    /// Trims the string value of the property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class TrimStringAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is string str)
            {
                str = str.Trim();
            }

            return true;
        }
    }
}
