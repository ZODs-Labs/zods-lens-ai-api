using ZODs.AI.Common.Enums;
using ZODs.AI.Common.Interfaces;

namespace ZODs.AI.Common.Models;

public sealed class AIContextMessage : IAIChatMessage
{
    public AIContextMessage(string content, ContextChatRole role)
    {
        Content = content;
        Role = role;
    }

    public AIContextMessage()
    {
    }

    public string Content { get; set; } = string.Empty;

    public int Length => Content.Length;

    public ContextChatRole Role { get; set; }
}
