using ZODs.Api.Repository;
using ZODs.Api.Repository.Interfaces;
using ZODs.Api.Service.Strategies.UserSubscriptionTransition.Interfaces;

namespace ZODs.Api.Service.Strategies.UserSubscriptionTransition;

public sealed class PastDueSubscriptionTransitionStrategy
    : BaseUserSubscriptionTransitionStrategy,
      IUserSubscriptionTransitionStrategy
{
    public PastDueSubscriptionTransitionStrategy(IUnitOfWork<ZodsContext> unitOfWork)
        : base(unitOfWork)
    {
    }

    public Task HandleUserSubscriptionTransitionAsync(
        string subscriptionStatus, 
        Guid userId, 
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}