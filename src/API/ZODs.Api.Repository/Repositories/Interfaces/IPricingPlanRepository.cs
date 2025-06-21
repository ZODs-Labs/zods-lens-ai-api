using ZODs.Api.Repository.Dtos;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Repository.Interfaces;

namespace ZODs.Api.Repository.Repositories.Interfaces
{
    public interface IPricingPlanRepository : IRepository<UserPricingPlan>
    {
        Task<ICollection<PricingPlanFeatureDto>> GetPricingPlanFeatures(
            Guid pricingPlanId,
            CancellationToken cancellationToken = default);
        Task<Guid> GetPricingPlanIdByTypeAsync(PricingPlanType planType, CancellationToken cancellationToken = default);
        Task<Guid> GetPricingPlanVariantByVariantIdAsync(int variantId, CancellationToken cancellationToken = default);
        Task<int> GetPricingPlanPaymentVariantId(PricingPlanType pricingPlanType, PricingPlanVariantType pricingPlanVariantType, CancellationToken cancellationToken = default);
        Task<UserPricingPlanDto?> GetUserPricingPlanInfo(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> IsPricingPlanVariantAssignedToUser(Guid pricingPlanVariantId, Guid userId, CancellationToken cancellationToken = default);
        Task<UserSubscriptionDto?> GetUserSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<string?> GetUserPaymentSubscriptionIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task DeleteUserPricingPlanAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Guid> GetPricingPlanVariantIdByTypeAndPricingPlanTypeAsync(PricingPlanType pricingPlanType, PricingPlanVariantType pricingPlanVariantType, CancellationToken cancellationToken = default);
        Task SetUserPricingPlanStatusesAsync(ICollection<Guid> userIds, string subscriptionStatus, CancellationToken cancellationToken = default);
        Task<string?> GetUserSubscriptionStatusAsync(Guid userId, CancellationToken cancellationToken = default);
        Task SetUserPricingPlansActiveStatusAsync(
            ICollection<Guid> userIds,
            bool isActive,
            CancellationToken cancellationToken = default);
        Task<Dictionary<string, string>> GetPricingPlanFeatureLimitationsAsync(Guid pricingPlanId, CancellationToken cancellationToken = default);
        Task<bool> IsPaidPricingPlanByVariantId(Guid pricingPlanVariantId, CancellationToken cancellationToken = default);
        Task<ICollection<FeatureLimitationIndex>> GetPricingPlanFeatureLimitationIndexesAsync(Guid pricingPlanId, CancellationToken cancellationToken = default);
    }
}