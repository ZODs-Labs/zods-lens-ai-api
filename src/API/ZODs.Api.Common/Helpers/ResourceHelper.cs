using System.Reflection;

namespace ZODs.Api.Common.Helpers;

public static class ResourceHelper
{
    /// <summary>
    /// Retrieves the content of an embedded resource.
    /// </summary>
    /// <param name="resourceName">Name of the resource. Generally, this is 'Namespace.Path.To.File.html'.</param>
    /// <returns>The content of the embedded resource as a string.</returns>
    public static string GetEmbeddedResource(string resourceName)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new ArgumentException($"Resource '{resourceName}' not found. Ensure the resource name is correct and the file has been set as an Embedded Resource.", nameof(resourceName));
        }

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}