using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZODs.Api.Common.Constants;
using ZODs.Api.Common.Helpers;
using ZODs.Api.Common.Interfaces;
using ZODs.Api.Repository;
using ZODs.Api.Repository.Dtos;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Repository.Interfaces;
using ZODs.Api.Repository.Repositories.Interfaces;
using ZODs.Api.Service.Dtos.User;
using ZODs.Api.Service.InputDtos.PricingPlan;
using ZODs.Api.Service.Managers;
using ZODs.Api.Service.Mappers;
using ZODs.Common.Constants;
using ZODs.Common.Extensions;
using ZODs.Common.Helpers;
using ZODs.Payment.Clients;

namespace ZODs.Api.Service.Services;

public sealed class PricingPlanService(
    IUnitOfWork<ZodsContext> unitOfWork,
    ICacheService cacheService,
    IBackgroundTaskQueue backgroundTaskQueue,
    IFeatureLimitationSyncManager featureLimitationSyncManager,
    ILogger<PricingPlanService> logger,
    IPaymentProcessorClient paymentProcessorClient,
    IUserInfoService userInfoService) : IPricingPlanService
{
    private readonly IUnitOfWork<ZodsContext> unitOfWork = unitOfWork;
    private readonly ICacheService cacheService = cacheService;
    private readonly IUserInfoService userInfoService = userInfoService;
    private readonly IBackgroundTaskQueue backgroundTaskQueue = backgroundTaskQueue;
    private readonly IFeatureLimitationSyncManager featureLimitationSyncManager = featureLimitationSyncManager;
    private readonly IPaymentProcessorClient paymentProcessorClient = paymentProcessorClient;
    private readonly ILogger<PricingPlanService> logger = logger;

    private IPricingPlanRepository PricingPlanRepository => this.unitOfWork.GetRepository<IPricingPlanRepository>();
    private IWorkspacesRepository WorkspacesRepository => this.unitOfWork.GetRepository<IWorkspacesRepository>();
    private IUserAICreditBalanceRepository UserAICreditBalanceRepository => unitOfWork.GetRepository<IUserAICreditBalanceRepository>();

    public async Task<UserPricingPlanDto> GetUserPricingPlanAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeyGenerator.GetUserPricingPlanIdKey(userId);
        var userPricingPlan = await this.cacheService.Get<UserPricingPlanDto>(cacheKey, cancellationToken);
        if (userPricingPlan == null)
        {
            userPricingPlan = await this.PricingPlanRepository.GetUserPricingPlanInfo(userId, cancellationToken);

            await this.cacheService.Set(cacheKey, userPricingPlan, options: null, cancellationToken);
        }

        if (userPricingPlan == null)
        {
            throw new KeyNotFoundException($"User pricing plan not found for user {userId}");
        }

        return userPricingPlan;
    }

    public async Task<Guid> GetUserPricingPlanIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var userPricingPlan = await this.GetUserPricingPlanAsync(userId, cancellationToken);
        if (userPricingPlan == null)
        {
            throw new KeyNotFoundException($"User pricing plan not found for user {userId}");
        }

        return userPricingPlan.PricingPlanId;
    }

    public async Task<IDictionary<string, object>> GetUserFeatureLimitationsUsage(
        Guid userId,
        Guid pricingPlanId,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeyGenerator.GetUserFeatureLimitationsUsageKey(userId);
        var userFeatureLimitationsUsage = await cacheService.Get<Dictionary<string, object>>(cacheKey, cancellationToken);

        if (userFeatureLimitationsUsage == null)
        {
            await featureLimitationSyncManager.SyncAllLimitationsAsync(userId, pricingPlanId, cancellationToken)
                                              .NoSync();

            userFeatureLimitationsUsage = await cacheService.Get<Dictionary<string, object>>(cacheKey, cancellationToken);

            // Fallback in case something went wrong
            userFeatureLimitationsUsage ??= [];
        }

        return userFeatureLimitationsUsage;
    }

    public async Task AssignPricingPlanToUserAsync(
        UpsertUserPricingPlanInputDto inputDto,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = inputDto.UserId;
            var variantId = inputDto.VariantId;
            var pricingPlanVariantId = await PricingPlanRepository.GetPricingPlanVariantByVariantIdAsync(
                   variantId,
                   cancellationToken);

            if (pricingPlanVariantId == default)
            {
                throw new KeyNotFoundException($"Pricing plan variant for variant id {variantId} not found.");
            }

            var isPaidPlan = await PricingPlanRepository.IsPaidPricingPlanByVariantId(pricingPlanVariantId, cancellationToken);
            UserPricingPlan? userPricingPlan = await this.PricingPlanRepository.FirstOrDefaultAsync(
                    x => x.UserId == userId,
                    cancellationToken: cancellationToken).NoSync();

            if (userPricingPlan != null)
            {
                // Some pricing plan is already assigned to user

                userPricingPlan = inputDto.ToPricingPlan();
                userPricingPlan.UserId = userId;
                userPricingPlan.PricingPlanVariantId = pricingPlanVariantId;
                userPricingPlan.IsPaid = isPaidPlan;
                userPricingPlan.IsActive = true;

                await this.PricingPlanRepository.Update(userPricingPlan, cancellationToken).NoSync();

                // Important step - invalidate user info cache to load fresh pricing plan features
                await this.InvalidateUserInfoCache(userId);
            }
            else
            {
                // No pricing plan is assigned to user
                // Create new user pricing plan entity

                userPricingPlan = inputDto.ToPricingPlan();
                userPricingPlan.UserId = userId;
                userPricingPlan.PricingPlanVariantId = pricingPlanVariantId;
                userPricingPlan.IsPaid = isPaidPlan;
                userPricingPlan.IsActive = true;

                await this.PricingPlanRepository.Insert(userPricingPlan, cancellationToken).NoSync();
            }

            await this.unitOfWork.CommitAsync(cancellationToken).NoSync();

            var userPricingPlanCacheKey = CacheKeyGenerator.GetUserPricingPlanIdKey(userId);
            await this.cacheService.Remove(userPricingPlanCacheKey, cancellationToken);

            // Important step to sync user info cache
            QueueUserInfoSync(userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to assign pricing plan to user.");
        }
    }

    public async Task UpdateUserPricingPlanAsync(
        UpsertUserPricingPlanInputDto inputDto,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = inputDto.UserId;
            var variantId = inputDto.VariantId;

            var userPricingPlan = await this.PricingPlanRepository.FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken: cancellationToken);

            if (userPricingPlan == null)
            {
                if (inputDto.SubscriptionStatus == SubscriptionStatus.Cancelled)
                {
                    // Regular cancellation flow 
                    // Pricing plan has been deleted before the webhook was received
                    return;
                }

                throw new KeyNotFoundException($"User pricing plan not found for user {userId}");
            }

            var pricingPlanVariantId = await PricingPlanRepository.GetPricingPlanVariantByVariantIdAsync(
                  variantId,
                  cancellationToken);

            if (pricingPlanVariantId == default)
            {
                throw new KeyNotFoundException($"Pricing plan variant for variant id {variantId} not found.");
            }

            await this.PricingPlanRepository.ExecuteUpdateAsync(
                userPricingPlan.Id,
                x => x
                      // Important step - set pricing plan variant id (actual plan)
                      .SetProperty(up => up.PricingPlanVariantId, pricingPlanVariantId)
                      .SetProperty(up => up.SubscriptionId, inputDto.SubscriptionId)
                      .SetProperty(up => up.SubscriptionStatus, inputDto.SubscriptionStatus)
                      .SetProperty(up => up.SubscriptionStatusFormatted, inputDto.SubscriptionStatusFormatted)
                      .SetProperty(up => up.VariantId, inputDto.VariantId)
                      .SetProperty(up => up.CustomerId, inputDto.CustomerId)
                      .SetProperty(up => up.NextBillingDate, inputDto.NextBillingDate)
                      .SetProperty(up => up.EndDate, inputDto.EndDate)
                      .SetProperty(up => up.IsPaid, up => up.IsPaid || inputDto.SubscriptionStatus == "active")
                      .SetProperty(up => up.ModifiedAt, DateTime.UtcNow),
                cancellationToken)
                .NoSync();

            await HandleDataSyncUponSubscriptionUpdate(userId, inputDto.SubscriptionStatus, cancellationToken).NoSync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update user pricing plan.");
        }
    }

    public async Task AssignFreePricingPlanToUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pricingPlanVariantId = await this.PricingPlanRepository.GetPricingPlanVariantIdByTypeAndPricingPlanTypeAsync(
                           PricingPlanType.Free,
                           PricingPlanVariantType.Free,
                           cancellationToken);
            if (pricingPlanVariantId == default)
            {
                throw new KeyNotFoundException($"Pricing plan variant for pricing plan type {PricingPlanType.Free} and pricing plan variant type {PricingPlanVariantType.Free} not found.");
            }

            var pricingPlanEntity = new UserPricingPlan
            {
                UserId = userId,
                PricingPlanVariantId = pricingPlanVariantId,
                IsPaid = false,
                IsActive = true,
            };
            await PricingPlanRepository.Insert(pricingPlanEntity, cancellationToken).NoSync();
            await unitOfWork.CommitAsync(cancellationToken).NoSync();

            await UserAICreditBalanceRepository.SetUserGpt3CreditsAsync(userId, 600, cancellationToken).NoSync();
            await unitOfWork.CommitAsync(cancellationToken).NoSync();

            // Important step to sync user info cache
            QueueUserInfoSync(userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to assign free pricing plan to user {userId}.", userId);
            throw;
        }
    }

    public async Task AssignNotPaidPricingPlanToUserAsync(
        Guid userId,
        PricingPlanVariantType pricingPlanVariantType,
        PricingPlanType pricingPlanType,
        CancellationToken cancellationToken = default)
    {
        var pricingPlanVariantId = await this.PricingPlanRepository.GetPricingPlanVariantIdByTypeAndPricingPlanTypeAsync(
                           pricingPlanType,
                           pricingPlanVariantType,
                           cancellationToken);

        if (pricingPlanVariantId == default)
        {
            throw new KeyNotFoundException($"Pricing plan variant for pricing plan type {pricingPlanType} and pricing plan variant type {pricingPlanVariantType} not found.");
        }

        var pricingPlanEntity = new UserPricingPlan
        {
            UserId = userId,
            PricingPlanVariantId = pricingPlanVariantId,
            IsPaid = false,
            IsActive = true,
        };
        await PricingPlanRepository.Insert(pricingPlanEntity, cancellationToken).NoSync();
        await unitOfWork.CommitAsync(cancellationToken).NoSync();

        // Important step to sync user info cache
        QueueUserInfoSync(userId);
    }

    public async Task CancelUserSubscriptionAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var subscriptionId = await PricingPlanRepository.GetUserPaymentSubscriptionIdAsync(userId, cancellationToken).NoSync();
        if (string.IsNullOrWhiteSpace(subscriptionId))
        {
            throw new KeyNotFoundException($"User {userId} does not have a subscription.");
        }

        await paymentProcessorClient.CancelSubscriptionAsync(subscriptionId, cancellationToken).NoSync();
    }

    public async Task DeactivateUserPricingPlansAsync(
        ICollection<Guid> userIds,
        CancellationToken cancellationToken)
    {
        await PricingPlanRepository.SetUserPricingPlansActiveStatusAsync(userIds, false, cancellationToken)
                                   .NoSync();
    }

    public async Task ActivateUserPricingPlansAsync(
        ICollection<Guid> userIds,
        CancellationToken cancellationToken)
    {
        await PricingPlanRepository.SetUserPricingPlansActiveStatusAsync(userIds, true, cancellationToken)
                                   .NoSync();
    }

    public async Task SetUserPricingPlanSubscriptionStatusesAsync(
        ICollection<Guid> userId,
        string subscriptionStatus,
        CancellationToken cancellationToken = default)
    {
        await PricingPlanRepository.SetUserPricingPlanStatusesAsync(userId, subscriptionStatus, cancellationToken)
                                   .NoSync();

        // Important step to sync user info cache
        userId.ToList().ForEach(QueueUserInfoSync);
    }

    public async Task<int> GetPricingPlanPaymentVariantIdAsync(
        PricingPlanType pricingPlanType,
        PricingPlanVariantType pricingPlanVariantType,
        CancellationToken cancellationToken = default)
    {
        var pricingPlanVariantId = await this.PricingPlanRepository.GetPricingPlanPaymentVariantId(
                 pricingPlanType,
                 pricingPlanVariantType,
                 cancellationToken);

        if (pricingPlanVariantId == default)
        {
            throw new KeyNotFoundException($"Pricing plan variant for pricing plan type {pricingPlanType} and pricing plan variant type {pricingPlanVariantType} not found.");
        }

        return pricingPlanVariantId;
    }

    public async Task<PricingPlanType> GetPricingPlanTypeByVariantIdAsync(
        int variantId,
        CancellationToken cancellationToken)
    {
        var pricingPlanType = await this.PricingPlanRepository.FirstOrDefaultAsync(
            x => x.VariantId == variantId,
            selector: x => x.PricingPlanVariant.PricingPlan.Type,
            cancellationToken: cancellationToken);

        return pricingPlanType;
    }

    public async Task<bool> HasUserPricingPlanAssignedAsync(
         Guid userId,
         CancellationToken cancellationToken = default)
    {
        var hasPricingPlan = await this.PricingPlanRepository.ExistsAsync(
                       x => x.UserId == userId,
                       cancellationToken: cancellationToken).NoSync();

        return hasPricingPlan;
    }

    public async Task<UserPricingPlanUsageDto> GetUserPricingPlanUsageAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var pricingPlanId = await GetUserPricingPlanIdAsync(userId, cancellationToken);
        var featureLimitationsUsage = await this.GetUserFeatureLimitationsUsage(userId, pricingPlanId, cancellationToken);

        var pricingPlanLimitations = await PricingPlanRepository.GetPricingPlanFeatureLimitationsAsync(pricingPlanId, cancellationToken);

        var maxPersonalSnippetsString = pricingPlanLimitations[FeatureLimitationKey.MaxPersonalSnippets];

        pricingPlanLimitations.TryGetValue(FeatureLimitationKey.MaxPersonalSnippetPrefixes, out var maxPersonalSnippetPrefixesString);
        pricingPlanLimitations.TryGetValue(FeatureLimitationKey.MaxWorkspaces, out var maxWorkspacesString);
        pricingPlanLimitations.TryGetValue(FeatureLimitationKey.MaxWorkspaceSnippets, out var maxWorkspaceSnippetsString);
        pricingPlanLimitations.TryGetValue(FeatureLimitationKey.MaxWorkspaceSnippetPrefixes, out var maxWorkspaceSnippetPrefixesString);
        pricingPlanLimitations.TryGetValue(FeatureLimitationKey.MaxWorkspaceInvites, out var maxWorkspaceInvitesString);
        pricingPlanLimitations.TryGetValue(FeatureLimitationKey.MaxAILenses, out var maxAILensesString);

        var personalSnippetsLeftObj = featureLimitationsUsage[FeatureLimitationUsage.MaxPersonalSnippets];

        featureLimitationsUsage.TryGetValue(FeatureLimitationUsage.MaxPersonalSnippetPrefixes, out var personalSnippetPrefixsLeftObj);
        featureLimitationsUsage.TryGetValue(FeatureLimitationUsage.MaxWorkspaces, out var workspacesLeftObj);
        featureLimitationsUsage.TryGetValue(FeatureLimitationUsage.MaxWorkspaceSnippets, out var workspaceSnippetsLeftObj);
        featureLimitationsUsage.TryGetValue(FeatureLimitationUsage.MaxWorkspaceSnippetPrefixes, out var workspaceSnippetPrefixesLeftObj);
        featureLimitationsUsage.TryGetValue(FeatureLimitationUsage.MaxWorkspaceInvites, out var workspaceInvitesLeftObj);
        featureLimitationsUsage.TryGetValue(FeatureLimitationUsage.MaxAILenses, out var aiLensesLeftObj);

        var snippetsLeftByWorkspaceId = JsonHelper.TryDeserialize<Dictionary<string, int>>(workspaceSnippetsLeftObj);
        var snippetPrefixesLeftByWorkspaceId = JsonHelper.TryDeserialize<Dictionary<string, int>>(workspaceSnippetPrefixesLeftObj);
        var invitesLeftByWorkspaceId = JsonHelper.TryDeserialize<Dictionary<string, int>>(workspaceInvitesLeftObj);

        var workspaceIds = snippetsLeftByWorkspaceId?.Keys.Select(Guid.Parse).ToArray() ?? Array.Empty<Guid>();
        var worksapcesIdToNameMap = await WorkspacesRepository.GetWorkspacesNameByIdsAsync(workspaceIds, cancellationToken);

        var snippetsLeftByWorkspaces = MapWorkspacesIdToName(snippetsLeftByWorkspaceId, worksapcesIdToNameMap);
        var snippetPrefixesLeftByWorkspaces = MapWorkspacesIdToName(snippetPrefixesLeftByWorkspaceId, worksapcesIdToNameMap);
        var invitesLeftByWorkspaces = MapWorkspacesIdToName(invitesLeftByWorkspaceId, worksapcesIdToNameMap);

        var maxPersonalSnippets = maxPersonalSnippetsString.TryParseIntOrZero();

        var usageDto = new UserPricingPlanUsageDto(
            personalSnippetsMax: maxPersonalSnippets == int.MaxValue ? -1 : maxPersonalSnippets,
            personalSnippetsLeft: personalSnippetsLeftObj.TryParseIntOrZero(),
            personalSnippetPrefixesMax: maxPersonalSnippetPrefixesString?.TryParseIntOrZero(),
            personalSnippetPrefixesLeft: maxPersonalSnippetPrefixesString?.TryParseIntOrZero(),
            workspacesMax: maxWorkspacesString?.TryParseIntOrZero(),
            workspacesLeft: workspacesLeftObj?.TryParseIntOrZero(),
            aiLensesMax: maxAILensesString?.TryParseIntOrZero(),
            aiLensesLeft: aiLensesLeftObj?.TryParseIntOrZero(),
            workspaceSnippetsMax: maxWorkspaceSnippetsString?.TryParseIntOrZero(),
            workspaceSnippetsLeft: snippetsLeftByWorkspaces,
            workspaceSnippetPrefixesMax: maxWorkspaceSnippetPrefixesString?.TryParseIntOrZero(),
            workspaceSnippetPrefixesLeft: snippetPrefixesLeftByWorkspaces,
            workspaceInvitesMax: maxWorkspaceInvitesString?.TryParseIntOrZero(),
            workspaceInvitesLeft: invitesLeftByWorkspaces);

        return usageDto;
    }

    private static Dictionary<string, int> MapWorkspacesIdToName(
        Dictionary<string, int>? workspaceIdsToValueMap,
        Dictionary<Guid, string> workspacesIdToNameMap)
    {
        if (workspaceIdsToValueMap == null)
        {
            return new Dictionary<string, int>();
        }

        var keys = new List<string>(workspaceIdsToValueMap.Keys);
        foreach (var key in keys)
        {
            if (Guid.TryParse(key, out var workspaceId) &&
                workspacesIdToNameMap.TryGetValue(workspaceId, out var workspaceName))
            {
                workspaceIdsToValueMap[workspaceName] = workspaceIdsToValueMap[key];
                workspaceIdsToValueMap.Remove(key);
            }
        }

        return workspaceIdsToValueMap.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
    }

    private void QueueUserInfoSync(Guid userId)
    {
        // Important step to sync user info cache
        backgroundTaskQueue.QueueBackgroundWorkItem(async (token, serviceProvider) =>
        {
            using var scope = serviceProvider.CreateScope();
            var userInfoService = scope.ServiceProvider.GetRequiredService<IUserInfoService>();

            await userInfoService.SyncUserInfoCacheAsync(userId, token);
        });
    }

    private void QueueUserSubscriptionStatusHandling(
         string subscriptionStatus,
         Guid userId)
    {
        backgroundTaskQueue.QueueBackgroundWorkItem(async (token, provider) =>
        {
            using var scope = provider.CreateScope();

            var subscriptionManager = scope.ServiceProvider.GetRequiredService<IUserSubscriptionManager>();
            await subscriptionManager.HandleUserSubscriptionStatusUpdateAsync(subscriptionStatus, userId, token);
        });
    }

    private async Task InvalidateUserInfoCache(Guid userId)
    {
        var userInfoCacheKey = CacheKeyGenerator.GetUserInfoKey(userId);
        await this.cacheService.Remove(userInfoCacheKey);
    }

    private async Task HandleDataSyncUponSubscriptionUpdate(
        Guid userId, 
        string subscriptionStatus,
        CancellationToken cancellationToken)
    {
        var userPricingPlanCacheKey = CacheKeyGenerator.GetUserPricingPlanIdKey(userId);
        await this.cacheService.Remove(userPricingPlanCacheKey, cancellationToken);

        // Important step to sync user info cache
        await userInfoService.SyncUserInfoCacheAsync(userId, cancellationToken);

        // User info sync will be handled by user subscription manager
        QueueUserSubscriptionStatusHandling(
           subscriptionStatus,
           userId);

        // Sync feature limitations
        featureLimitationSyncManager.QueueAllLimitationsSync(userId);

    }
}