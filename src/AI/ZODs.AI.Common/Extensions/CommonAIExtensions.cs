using ZODs.AI.Common.Enums;

namespace ZODs.AI.Common.Extensions;

public static class CommonAIExtensions
{
    public static string ToEnumString(this ContextChatRole role) => role switch
    {
        ContextChatRole.System => "system",
        ContextChatRole.Assistant => "assistant",
        ContextChatRole.User => "user",
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
    };
}
