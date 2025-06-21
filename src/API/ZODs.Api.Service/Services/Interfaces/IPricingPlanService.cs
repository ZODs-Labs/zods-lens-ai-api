using ZODs.Api.Repository.Dtos;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Service.Dtos.User;
using ZODs.Api.Service.InputDtos.PricingPlan;

namespace ZODs.Api.Service
{
    public interface IPricingPlanService
    {
        Task AssignPricingPlanToUserAsync(
            UpsertUserPricingPlanInputDto inputDto,
            CancellationToken cancellationToken);

        Task<UserPricingPlanDto> GetUserPricingPlanAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Guid> GetUserPricingPlanIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<int> GetPricingPlanPaymentVariantIdAsync(PricingPlanType pricingPlanType, PricingPlanVariantType pricingPlanVariantType, CancellationToken cancellationToken = default);
        Task UpdateUserPricingPlanAsync(UpsertUserPricingPlanInputDto inputDto, CancellationToken cancellationToken);
        Task AssignNotPaidPricingPlanToUserAsync(Guid userId, PricingPlanVariantType pricingPlanVariantType, PricingPlanType pricingPlanType, CancellationToken cancellationToken = default);
        Task SetUserPricingPlanSubscriptionStatusesAsync(ICollection<Guid> userId, string subscriptionStatus, CancellationToken cancellationToken = default);
        Task DeactivateUserPricingPlansAsync(ICollection<Guid> userIds, CancellationToken cancellationToken);
        Task ActivateUserPricingPlansAsync(ICollection<Guid> userIds, CancellationToken cancellationToken);
        Task CancelUserSubscriptionAsync(Guid userId, CancellationToken cancellationToken);
        Task<UserPricingPlanUsageDto> GetUserPricingPlanUsageAsync(Guid userId, CancellationToken cancellationToken);
        Task AssignFreePricingPlanToUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<PricingPlanType> GetPricingPlanTypeByVariantIdAsync(int variantId, CancellationToken cancellationToken);
        Task<bool> HasUserPricingPlanAssignedAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}