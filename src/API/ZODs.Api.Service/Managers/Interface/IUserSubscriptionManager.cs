namespace ZODs.Api.Service.Managers;

public interface IUserSubscriptionManager
{
    Task HandleUserSubscriptionStatusUpdateAsync(string subscriptionStatus, Guid userId, CancellationToken cancellationToken);
}