using ZODs.AI.Common.Enums;
using ZODs.AI.Common.Interfaces;

namespace ZODs.AI.Together.Models;

public sealed class TogetherMessage : IAIChatMessage
{
    public TogetherMessage(string content, ContextChatRole role)
    {
        Content = content;
        Role = role;
    }

    public string Content { get; set; } = string.Empty;

    public ContextChatRole Role { get; set; }
}
