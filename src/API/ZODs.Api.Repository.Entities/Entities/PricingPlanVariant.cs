using ZODs.Api.Repository.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Repository.Entities;

public sealed class PricingPlanVariant : BaseEntity
{
    [Required]
    public decimal Price { get; set; }

    [Required]
    public PricingPlanVariantType VariantType { get; set; }

    [Required]
    public int VariantId { get; set; }

    public Guid PricingPlanId { get; set; }
    public PricingPlan PricingPlan { get; set; } = null!;

    public ICollection<UserPricingPlan> UserPricingPlans { get; set; } = new List<UserPricingPlan>();
}