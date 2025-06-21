using ZODs.Api.Repository.Common;
using ZODs.Api.Repository.Dtos;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Repository.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using ZODs.Common.Extensions;

namespace ZODs.Api.Repository.Repositories
{
    public sealed class PricingPlanRepository : Repository<UserPricingPlan, ZodsContext>, IPricingPlanRepository
    {
        public PricingPlanRepository(ZodsContext context)
            : base(context)
        {
        }

        public async Task<ICollection<PricingPlanFeatureDto>> GetPricingPlanFeatures(
            Guid pricingPlanId,
            CancellationToken cancellationToken = default)
        {
            var query = Context.PricingPlans.Where(x => x.Id == pricingPlanId)
                                            .SelectMany(x => x.PlanFeatures)
                                            .Select(x => new PricingPlanFeatureDto
                                            {
                                                Index = x.Feature.FeatureIndex,
                                                Key = x.Feature.Key,
                                                Limitations = x.Limitations.Select(l => new PricingPlanFeatureLimitationDto
                                                {
                                                    Index = l.FeatureLimitation.Index,
                                                    Key = l.FeatureLimitation.Key,
                                                    Value = l.Value
                                                })
                                            });

            var pricingPlanFeatures = await query.ToArrayAsync(cancellationToken);
            foreach (var pricingPlanFeature in pricingPlanFeatures)
            {
                foreach (var limitation in pricingPlanFeature.Limitations)
                {
                    limitation.Value = FeatureLimitationValueTypeMapper.MapValue(limitation.Index, limitation.Value.ToString() ?? string.Empty);
                }
            }

            return pricingPlanFeatures;
        }

        public async Task<UserPricingPlanDto?> GetUserPricingPlanInfo(Guid userId, CancellationToken cancellationToken = default)
        {
            var pricingPlan = await Context.UserPricingPlans.Where(x => x.UserId == userId && x.IsActive)
                                                 .Select(x => new UserPricingPlanDto
                                                 {
                                                     PricingPlanId = x.PricingPlanVariant.PricingPlanId,
                                                     PricingPlanType = x.PricingPlanVariant.PricingPlan.Type,
                                                     SubscriptionStatus = x.SubscriptionStatus,
                                                     RegistrationType = x.User.RegistrationType,
                                                     HasActivePricingPlan = x.IsActive,
                                                     EndDate = x.EndDate,
                                                     IsPaidPlan = x.IsPaid,
                                                 })
                                                 .FirstOrDefaultAsync(cancellationToken);
            return pricingPlan;
        }

        public async Task<Guid> GetPricingPlanIdByTypeAsync(
            PricingPlanType planType,
            CancellationToken cancellationToken = default)
        {
            var pricingPlanId = await Context.PricingPlans.Where(x => x.Type == planType)
                                                          .Select(x => x.Id)
                                                          .FirstOrDefaultAsync(cancellationToken);

            return pricingPlanId;
        }

        public async Task<Guid> GetPricingPlanVariantByVariantIdAsync(
            int variantId,
            CancellationToken cancellationToken = default)
        {
            var pricingPlanVariantId = await Context.PricingPlanVariants.Where(x => x.VariantId == variantId)
                                                                        .Select(x => x.Id)
                                                                        .FirstOrDefaultAsync(cancellationToken);

            return pricingPlanVariantId;
        }

        public async Task<Guid> GetPricingPlanVariantIdByTypeAndPricingPlanTypeAsync(
            PricingPlanType pricingPlanType,
            PricingPlanVariantType pricingPlanVariantType,
            CancellationToken cancellationToken = default)
        {
            var pricingPlanVariantId = await Context.PricingPlanVariants.Where(x => x.PricingPlan.Type == pricingPlanType &&
                                                                                    x.VariantType == pricingPlanVariantType)
                                                                        .Select(x => x.Id)
                                                                        .FirstOrDefaultAsync(cancellationToken);

            return pricingPlanVariantId;
        }

        public async Task<bool> IsPricingPlanVariantAssignedToUser(
            Guid pricingPlanVariantId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var isPricingPlanVariantAssignedToUser = await Context.UserPricingPlans.AnyAsync(x => x.UserId == userId &&
                                                                                                  x.PricingPlanVariantId == pricingPlanVariantId,
                                                                                             cancellationToken);

            return isPricingPlanVariantAssignedToUser;
        }

        public async Task<int> GetPricingPlanPaymentVariantId(
            PricingPlanType pricingPlanType,
            PricingPlanVariantType pricingPlanVariantType,
            CancellationToken cancellationToken = default)
        {
            var pricingPlanVariantId = await Context.PricingPlanVariants.Where(x => x.PricingPlan.Type == pricingPlanType &&
                                                                                    x.VariantType == pricingPlanVariantType)
                                                                         .Select(x => x.VariantId)
                                                                         .FirstOrDefaultAsync(cancellationToken);

            return pricingPlanVariantId;
        }

        public async Task<UserSubscriptionDto?> GetUserSubscriptionAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var userSubscription = await Context.UserPricingPlans.Where(x => x.UserId == userId)
                                                                 .Select(x => new UserSubscriptionDto
                                                                 {
                                                                     SubscriptionStatus = x.SubscriptionStatus,
                                                                     SubscriptionStatusFormatted = x.SubscriptionStatusFormatted ?? "Inactive",
                                                                     PricingPlanType = x.PricingPlanVariant.PricingPlan.Type,
                                                                     PricingPlanVariantType = x.PricingPlanVariant.VariantType,
                                                                     NextBillingDate = x.NextBillingDate,
                                                                     EndDate = x.EndDate,
                                                                     UpdatePaymentUrl = x.User.UpdatePaymentMethodUrl,
                                                                 })
                                                                 .FirstOrDefaultAsync(cancellationToken);

            return userSubscription;
        }

        public async Task<string?> GetUserPaymentSubscriptionIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var subscriptionId = await Context.UserPricingPlans.Where(x => x.UserId == userId)
                                                               .Select(x => x.SubscriptionId)
                                                               .FirstOrDefaultAsync(cancellationToken);

            return subscriptionId;
        }

        public async Task DeleteUserPricingPlanAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            await Context.UserPricingPlans.Where(x => x.UserId == userId)
                                          .ExecuteDeleteAsync(cancellationToken)
                                          .NoSync();
        }

        public Task SetUserPricingPlanStatusesAsync(
            ICollection<Guid> userIds,
            string subscriptionStatus,
            CancellationToken cancellationToken = default)
        {
            return Context.UserPricingPlans.Where(x => userIds.Contains(x.UserId))
                                          .ExecuteUpdateAsync(x => x.SetProperty(p => p.SubscriptionStatus, p => subscriptionStatus),
                                                              cancellationToken);
        }

        public Task<string?> GetUserSubscriptionStatusAsync(
            Guid userId,
                       CancellationToken cancellationToken = default)
        {
            return Context.UserPricingPlans.Where(x => x.UserId == userId)
                                           .Select(x => x.SubscriptionStatus)
                                           .FirstOrDefaultAsync(cancellationToken);
        }

        public Task SetUserPricingPlansActiveStatusAsync(
            ICollection<Guid> userIds,
            bool isActive,
            CancellationToken cancellationToken = default)
        {
            return Context.UserPricingPlans.Where(x => userIds.Contains(x.UserId))
                                           .ExecuteUpdateAsync(x => x.SetProperty(p => p.IsActive, p => isActive),
                                                                                 cancellationToken);
        }

        public async Task<Dictionary<string, string>> GetPricingPlanFeatureLimitationsAsync(
            Guid pricingPlanId,
            CancellationToken cancellationToken = default)
        {
            var query = Context.PricingPlans.Where(x => x.Id == pricingPlanId)
                                            .SelectMany(x => x.PlanFeatures)
                                            .SelectMany(x => x.Limitations)
                                            .Select(x => new
                                            {
                                                x.FeatureLimitation.Key,
                                                x.Value
                                            });

            var pricingPlanFeatureLimitations = await query.ToDictionaryAsync(x => x.Key, x => x.Value, cancellationToken).NoSync();
            return pricingPlanFeatureLimitations;
        }

        public async Task<bool> IsPaidPricingPlanByVariantId(Guid pricingPlanVariantId, CancellationToken cancellationToken = default)
        {
            var isPaid = await Context.PricingPlanVariants.Where(x => x.Id == pricingPlanVariantId)
                                                          .Select(x => x.PricingPlan.Type != PricingPlanType.Free)
                                                          .FirstOrDefaultAsync(cancellationToken);

            return isPaid;
        }

        public async Task<ICollection<FeatureLimitationIndex>> GetPricingPlanFeatureLimitationIndexesAsync(
            Guid pricingPlanId,
            CancellationToken cancellationToken = default)
        {
            var featureLimitationIndexes = await Context.PricingPlans.Where(x => x.Id == pricingPlanId)
                                 .SelectMany(x => x.PlanFeatures.SelectMany(pf => pf.Limitations.Select(l => l.FeatureLimitation.Index)))
                                 .Distinct()
                                 .ToArrayAsync(cancellationToken);

            return featureLimitationIndexes;
        }
    }
}