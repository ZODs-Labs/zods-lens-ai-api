using ZODs.AI.Common.Enums;

namespace ZODs.AI.Common.Interfaces;

public interface IAIChatMessage
{
    public string Content { get; set; }

    public ContextChatRole Role { get; set; }
}
