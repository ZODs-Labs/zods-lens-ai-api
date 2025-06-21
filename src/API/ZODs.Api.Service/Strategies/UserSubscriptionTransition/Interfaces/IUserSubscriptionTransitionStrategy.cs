namespace ZODs.Api.Service.Strategies.UserSubscriptionTransition.Interfaces;

public interface IUserSubscriptionTransitionStrategy
{
    Task HandleUserSubscriptionTransitionAsync(
          string subscriptionStatus,
          Guid userId,
          CancellationToken cancellationToken);
}