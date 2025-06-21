using Azure.AI.OpenAI;
using ZODs.AI.Common.Models;
using OpenAIChatRole = Azure.AI.OpenAI.ChatRole;
using ContextChatRole = ZODs.AI.Common.Enums.ContextChatRole;

namespace ZODs.AI.OpenAI.Extensions;

public static class OpenAIMappingExtensions
{
    public static ICollection<ChatMessage> ToChatMessages(this ICollection<AIContextMessage> messages)
    {
        return messages.Select(m => new ChatMessage
        {
            Content = m.Content,
            Role = m.Role.ToOpenAIRole(),
        }).ToList();
    }

    public static AIContextMessage ToAIContextMessage(this ChatMessage message)
    {
        return new AIContextMessage
        {
            Content = message.Content,
            Role = message.Role.ToChatRole(),
        };
    }

    private static OpenAIChatRole ToOpenAIRole(this ContextChatRole role)
    {
        return role switch
        {
            ContextChatRole.User => OpenAIChatRole.User,
            ContextChatRole.Assistant => OpenAIChatRole.Assistant,
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
        };
    }

    private static ContextChatRole ToChatRole(this OpenAIChatRole role)
    {
        if (role == OpenAIChatRole.User)
        {
            return ContextChatRole.User;
        }
        else if (role == OpenAIChatRole.Assistant)
        {
            return ContextChatRole.Assistant;
        }

        throw new ArgumentOutOfRangeException(nameof(role), role, null);
    }
}
