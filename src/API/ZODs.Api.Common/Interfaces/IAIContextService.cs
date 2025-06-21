using ZODs.AI.Common.Models;

namespace ZODs.Api.Common.Interfaces;

public interface IAIContextService
{
    void AddMessagesToContextAsBackgroundTaskAsync(
    Guid chatId,
    ICollection<AIContextMessage> newMessages);

    Task AddMessagesToContextAsync(Guid chatId, ICollection<AIContextMessage> newMessages);
    Task<ICollection<AIContextMessage>> GetContextMessages(Guid chatId);
}
