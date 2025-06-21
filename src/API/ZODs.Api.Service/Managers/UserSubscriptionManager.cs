using ZODs.Api.Common.Enums;
using ZODs.Api.Repository.Interfaces;
using ZODs.Api.Repository;
using ZODs.Api.Repository.Repositories.Interfaces;
using ZODs.Common.Constants;
using ZODs.Api.Service.Factories.Interfaces;
using ZODs.Common.Extensions;

namespace ZODs.Api.Service.Managers;

public sealed class UserSubscriptionManager : IUserSubscriptionManager
{
    private readonly IUnitOfWork<ZodsContext> _unitOfWork;
    private readonly IUserSubscriptionTransitionStrategyFactory userSubscriptionTransitionStrategyFactory;
    private readonly IUserInfoService userInfoService;

    public UserSubscriptionManager(
        IUnitOfWork<ZodsContext> unitOfWork,
        IUserSubscriptionTransitionStrategyFactory userSubscriptionTransitionStrategyFactory,
        IUserInfoService userInfoService)
    {
        _unitOfWork = unitOfWork;
        this.userSubscriptionTransitionStrategyFactory = userSubscriptionTransitionStrategyFactory;
        this.userInfoService = userInfoService;
    }

    private IPricingPlanRepository PricingPlanRepository => _unitOfWork.GetRepository<IPricingPlanRepository>();

    public async Task HandleUserSubscriptionStatusUpdateAsync(
        string subscriptionStatus,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var userSubscriptionTransition = await GetUserSubscriptionTransitionAsync(subscriptionStatus, userId, cancellationToken);
        if (userSubscriptionTransition == null)
        {
            // No transition to handle
            return;
        }

        var strategy = userSubscriptionTransitionStrategyFactory.Create(userSubscriptionTransition.Value);

        await strategy.HandleUserSubscriptionTransitionAsync(subscriptionStatus, userId, cancellationToken).NoSync();
        await userInfoService.SyncUserInfoCacheAsync(userId, cancellationToken);
    }

    private async Task<UserSubscriptionTransition?> GetUserSubscriptionTransitionAsync(
        string subscriptionStatus,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var currentSubscriptionStatus = await PricingPlanRepository.GetUserSubscriptionStatusAsync(userId, cancellationToken);
        if (string.IsNullOrWhiteSpace(currentSubscriptionStatus) ||
            string.Equals(subscriptionStatus, currentSubscriptionStatus, StringComparison.InvariantCultureIgnoreCase))
        {
            // If there is no current subscription status
            // or if the current subscription status is the same as the new subscription status
            return null;
        }

        if (subscriptionStatus == SubscriptionStatus.Active &&
           currentSubscriptionStatus == SubscriptionStatus.Expired)
        {
            return UserSubscriptionTransition.ExpiredToActive;
        }

        return subscriptionStatus switch
        {
            SubscriptionStatus.PastDue => UserSubscriptionTransition.ToPastDue,
            SubscriptionStatus.Unpaid => UserSubscriptionTransition.ToUnpaid,
            SubscriptionStatus.Cancelled => UserSubscriptionTransition.ToCancelled,
            SubscriptionStatus.Expired => UserSubscriptionTransition.ToExpired,
            _ => null
        };
    }
}