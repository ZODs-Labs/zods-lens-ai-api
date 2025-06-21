using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ZODs.Common.Attributes;

public class ValidPasswordComplexityAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value == null)
            return true; // Allow null values if desired

        var password = value.ToString();

        if (password.Length < 8)
            return false;

        if (!ContainsUpperCaseLetter(password))
            return false;

        if (!ContainsLowerCaseLetter(password))
            return false;

        if (!ContainsNonAlphanumericChar(password))
            return false;

        return true;
    }

    private bool ContainsUpperCaseLetter(string input)
    {
        return Regex.IsMatch(input, @"[A-Z]");
    }

    private bool ContainsLowerCaseLetter(string input)
    {
        return Regex.IsMatch(input, @"[a-z]");
    }

    private bool ContainsNonAlphanumericChar(string input)
    {
        return Regex.IsMatch(input, @"[^0-9a-zA-Z]");
    }

    public override string FormatErrorMessage(string name)
    {
        return $"{name} must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, and one non-alphanumeric character.";
    }
}
