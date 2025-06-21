using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Repository.Entities;

public sealed class UserPricingPlan : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid PricingPlanVariantId { get; set; }
    public PricingPlanVariant PricingPlanVariant { get; set; } = null!;

    [Required]
    public bool IsPaid { get; set; }

    [Required]
    public bool IsActive { get; set; }

    [MaxLength(20)]
    public string? SubscriptionStatus { get; set; } = null!;

    [MaxLength(30)]
    public string? SubscriptionStatusFormatted { get; set; } = null!;

    // Payment processor fields
    [MaxLength(100)]
    public string? SubscriptionId { get; set; } = null!;

    [MaxLength(100)]
    public int? CustomerId { get; set; }

    [MaxLength(100)]
    public int? VariantId { get; set; }

    public DateTime? NextBillingDate { get; set; }

    public DateTime? EndDate { get; set; }

    public decimal? DiscountAmount { get; set; }

    public UserPricingPlan(
        Guid userId,
        Guid pricingPlanVariantId,
        string subscriptionStatus,
        string subscriptionStatusFormatted,
        string subscriptionId,
        int customerId,
        int variantId,
        DateTime? endDate,
        DateTime? nextBillingDate,
        decimal? discountAmount)
    {
        UserId = userId;
        PricingPlanVariantId = pricingPlanVariantId;
        SubscriptionStatus = subscriptionStatus;
        SubscriptionStatusFormatted = subscriptionStatusFormatted;
        SubscriptionId = subscriptionId;
        CustomerId = customerId;
        VariantId = variantId;
        EndDate = endDate;
        NextBillingDate = nextBillingDate;
        DiscountAmount = discountAmount;
    }

    public UserPricingPlan()
    {
    }
}

