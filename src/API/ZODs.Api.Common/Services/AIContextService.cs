using Microsoft.Extensions.DependencyInjection;
using ZODs.AI.Common.Models;
using ZODs.Api.Common.Interfaces;

namespace ZODs.Api.Common.Services;

public sealed class AIContextService(
    ICacheService cahceService,
    IBackgroundTaskQueue backgroundTaskQueue) : IAIContextService
{
    private readonly IBackgroundTaskQueue backgroundTaskQueue = backgroundTaskQueue;
    private readonly ICacheService cahceService = cahceService;
    private const string RedisKeyPrefix = "ChatContext:";
    private static readonly int MAX_MESSAGES = 6;

    public async Task<ICollection<AIContextMessage>> GetContextMessages(Guid chatId)
    {
        return await GetContextMessagesAsync(chatId);
    }

    public void AddMessagesToContextAsBackgroundTaskAsync(
        Guid chatId,
        ICollection<AIContextMessage> newMessages)
    {
        backgroundTaskQueue.QueueBackgroundWorkItem(async (_, serviceProvider) =>
        {
            var scope = serviceProvider.CreateScope();
            var contextService = scope.ServiceProvider.GetRequiredService<IAIContextService>();
            await contextService.AddMessagesToContextAsync(chatId, newMessages);
        });
    }

    public async Task AddMessagesToContextAsync(
        Guid chatId,
        ICollection<AIContextMessage> newMessage)
    {
        var existingMessages = await GetContextMessagesAsync(chatId);
        existingMessages.AddRange(newMessage);

        if (existingMessages.Count > MAX_MESSAGES)
        {
            existingMessages.RemoveAt(0);
        }

        for (var i = 0; i < existingMessages.Count; i++)
        {
            var message = existingMessages.ElementAt(i);
            var messageLength = GetMessageLengthByPosition(i, existingMessages.Count);
            var messageText = message.Content;

            if (messageText.Length > messageLength)
            {
                messageText = messageText[..messageLength];
            }

            existingMessages[i].Content = messageText;
        }

        var contextTimeToLive = TimeSpan.FromHours(48);

        await cahceService.SetListAsync($"{RedisKeyPrefix}{chatId}", existingMessages, contextTimeToLive);
    }

    private async Task<List<AIContextMessage>> GetContextMessagesAsync(
        Guid chatId)
    {
        var messages = await cahceService.GetItemsFromListAsync<AIContextMessage>($"{RedisKeyPrefix}{chatId}");
        return messages.ToList();
    }

    private static int GetMessageLengthByPosition(int position, int length)
    {
        if (position == length - 1)
        {
            // First message should store 2000 characters
            return 2000;
        }
        else if (position == length - 2)
        {
            // Second message should store 1500 characters
            return 1500;
        }
        else
        {
            // The rest of the messages should store 500 characters
            return 500;
        }
    }
}
