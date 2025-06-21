using ZODs.Api.Repository.Interfaces;
using ZODs.Api.Repository;
using ZODs.Api.Service.Strategies.UserSubscriptionTransition.Interfaces;

namespace ZODs.Api.Service.Strategies.UserSubscriptionTransition;

public sealed class ExpiredSubscriptionTransitionStrategy
    : BaseUserSubscriptionTransitionStrategy,
      IUserSubscriptionTransitionStrategy
{
    private readonly IUserInfoService userInfoService;
    private readonly IPricingPlanService pricingPlanService;

    public ExpiredSubscriptionTransitionStrategy(
        IUnitOfWork<ZodsContext> unitOfWork,
        IPricingPlanService pricingPlanService,
        IUserInfoService userInfoService)
        : base(unitOfWork)
    {
        this.pricingPlanService = pricingPlanService;
        this.userInfoService = userInfoService;
    }

    private IWorkspacesRepository WorkspacesRepository => unitOfWork.GetRepository<IWorkspacesRepository>();

    public async Task HandleUserSubscriptionTransitionAsync(
        string subscriptionStatus,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        /*
            Handle transition to Expired subscription status.
            This transition happens when user subscription expiration 
            occurs for any reason (e.g. grace period expiration, etc...).
            
            User should not be able to access any of the business modules 
            of the application, except for the Profile module.

            Also, all users who inherited their pricing plans from workspaces
            owned by this user should have their pricing plans deactivated.
         */

        // Deactivate pricing plans only for users who inherited their pricing plans
        // from workspaces owned by this user - by accepting invitations to those workspaces.
        // If user has their own pricing plan or has pricing plan inheritance from workspaces
        // owned by other users, then their pricing plan should not be deactivated.
        await DeactivatePricingPlanForAllExclusiveWorkspaceMembersAsync(userId, cancellationToken);

        // Deactivate all workspaces owned by the user
        await WorkspacesRepository.SetUserWorkspacesActiveStatusAsync(userId, isActive: false, cancellationToken);
    }

    private async Task DeactivatePricingPlanForAllExclusiveWorkspaceMembersAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        // Get all users that are exclusive members of workspaces owned by the user.
        // Exclusive members are those who are members of only workspaces owned by specified user.
        var allExclusiveWorkspaceMemberUserIds = await WorkspacesRepository
            .GetAllExclusiveMembersOfOwnerWorkspacesAsync(userId, cancellationToken);

        await pricingPlanService.DeactivateUserPricingPlansAsync(allExclusiveWorkspaceMemberUserIds, cancellationToken);

        foreach (var memberUserId in allExclusiveWorkspaceMemberUserIds)
        {
            // Important: Re-sync user info cache for all users who inherited pricing plan
            await userInfoService.SyncUserInfoCacheAsync(memberUserId, cancellationToken);
        }
    }
}