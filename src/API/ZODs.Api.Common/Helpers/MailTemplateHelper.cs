using System.Text.RegularExpressions;

namespace ZODs.Api.Common.Helpers;

public static class MailTemplateHelper
{
    /// <summary>
    /// Formats an HTML email template with provided parameters.
    /// </summary>
    /// <param name="resourceName">Name of the embedded resource (HTML template).</param>
    /// <param name="parameters">The parameters to be inserted into the template.</param>
    /// <returns>A formatted HTML email content.</returns>
    public static string GetHtmlMailTemplateAsString(string resourceName, Dictionary<string, string> parameters)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
        {
            throw new ArgumentException("Resource name cannot be null or empty.", nameof(resourceName));
        }

        string templateContent = ResourceHelper.GetEmbeddedResource(resourceName);

        if (parameters == null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        foreach (var param in parameters)
        {
            string pattern = $"\\{{{{({Regex.Escape(param.Key)})}}}}"; // matches {{key}} pattern
            string replacement = param.Value ?? string.Empty; // If value is null, replace with an empty string.

            templateContent = Regex.Replace(templateContent, pattern, m => replacement);
        }

        return templateContent;
    }
}