using ZODs.Api.Repository;
using ZODs.Api.Repository.Interfaces;
using ZODs.Api.Service.Strategies.UserSubscriptionTransition.Interfaces;

namespace ZODs.Api.Service.Strategies.UserSubscriptionTransition;

public sealed class ExpiredToActiveSubscriptionTransitionStrategy
    : BaseUserSubscriptionTransitionStrategy,
      IUserSubscriptionTransitionStrategy
{
    public ExpiredToActiveSubscriptionTransitionStrategy(IUnitOfWork<ZodsContext> unitOfWork)
        : base(unitOfWork)
    {
    }

    private IWorkspacesRepository WorkspacesRepository => unitOfWork.GetRepository<IWorkspacesRepository>();

    public async Task HandleUserSubscriptionTransitionAsync(
        string subscriptionStatus,
        Guid userId,
        CancellationToken cancellationToken)
    {
        await RevertPricingPlanForAllExclusiveWorkspaceMembersAsync(userId, cancellationToken);

        // Activate all workspaces owned by the user
        await WorkspacesRepository.SetUserWorkspacesActiveStatusAsync(userId, isActive: true, cancellationToken);
    }

    private async Task RevertPricingPlanForAllExclusiveWorkspaceMembersAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        // Get all users that are exclusive members of workspaces owned by the user.
        // Exclusive members are those who are members of only workspaces owned by specified user.
        var allExclusiveWorkspaceMemberUserIds = await WorkspacesRepository
            .GetAllExclusiveMembersOfOwnerWorkspacesAsync(userId, cancellationToken);

        await PricingPlanRepository.SetUserPricingPlansActiveStatusAsync(
            allExclusiveWorkspaceMemberUserIds,
            isActive: true,
            cancellationToken);
    }
}