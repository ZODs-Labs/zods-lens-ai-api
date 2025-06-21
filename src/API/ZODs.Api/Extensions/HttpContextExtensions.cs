using ZODs.Api.Common.Constants;

namespace ZODs.Api.Extensions;

public static class HttpContextExtensions
{
    public static void AddAIModelTokensUsageHeaders(this HttpContext httpContext, int tokens, string model)
    {
        httpContext.Items.Add(CustomHeaders.UsedTokensHeader, tokens.ToString());
        httpContext.Items.Add(CustomHeaders.AIModelHeader, model);
    }

    // Retrieves the AI Model Header from the provided HttpContext object.
    // 
    // Parameters:
    //   httpContext:
    //     The HttpContext object from which the AI Model Header will be retrieved.
    //
    // Returns:
    //     A string containing the value of the AI Model Header, if present. Otherwise, an empty string is returned.
    public static string GetAIModelHeader(this HttpContext httpContext)
    {
        if (httpContext.Items.TryGetValue(CustomHeaders.AIModelHeader, out var aiModelHeader) && 
            aiModelHeader.ToString() is string aiModelString)
        {
            return aiModelString; 
        }

        return string.Empty; 
    }

    public static int GetUsedTokensHeader(this HttpContext httpContext)
    {
        if (httpContext.Items.TryGetValue(CustomHeaders.UsedTokensHeader, out var usedTokensHeader) &&
            int.TryParse(usedTokensHeader?.ToString(), out int usedTokensInteger))
        {
            return usedTokensInteger;
        }

        return 0;
    }
}
