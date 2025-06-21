using System.Text.Json;

namespace ZODs.Common.Helpers
{
    public static class JsonHelper
    {
        public static T? TryDeserialize<T>(object? json)
        {
            var jsonAsString = json?.ToString();
            if (string.IsNullOrWhiteSpace(jsonAsString))
            {
                return default;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(jsonAsString);
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}